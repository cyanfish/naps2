using System.Text;

namespace NAPS2.Scan.Internal.Sane;

public class SaneOptionParser
{
    private static readonly Dictionary<string, SaneUnit> Units = new Dictionary<string, SaneUnit>
    {
        { "pel", SaneUnit.Pixel },
        { "bit", SaneUnit.Bit },
        { "mm", SaneUnit.Mm },
        { "dpi", SaneUnit.Dpi },
        { "%", SaneUnit.Percent },
        { "us", SaneUnit.Microsecond }
    };

    private StreamReader? _input;
    private SaneOptionCollection? _options;
    private SaneOption? _lastOption;
    private OptionParseState _state;
    private string? _line;

    public SaneOptionCollection Parse(StreamReader streamReader)
    {
        _input = streamReader;
        _options = new SaneOptionCollection();
        _state = OptionParseState.NonDeviceOptions;
        _line = null;

        NextLine();
        while (_line != null)
        {
            switch (_state)
            {
                case OptionParseState.NonDeviceOptions:
                    if (_line.StartsWith("Options specific to device", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _state = OptionParseState.LookingForOption;
                    }
                    NextLine();
                    break;

                case OptionParseState.LookingForOption:
                    if (_line.StartsWith("    -", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ParseOption();
                        _state = OptionParseState.ReadingDescription;
                    }
                    NextLine();
                    break;

                case OptionParseState.ReadingDescription:
                    if (_line.StartsWith("        ", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (_lastOption == null)
                        {
                            // TODO: Seems like this case is possible to hit, should avoid
                            throw new InvalidOperationException("Couldn't read option");
                        }
                        string descLine = _line.Substring(8).Trim();
                        if (_lastOption.Desc == null)
                        {
                            _lastOption.Desc = descLine;
                        }
                        else
                        {
                            _lastOption.Desc += " " + descLine;
                        }
                        NextLine();
                    }
                    else
                    {
                        _state = OptionParseState.LookingForOption;
                    }
                    break;
            }
        }

        return _options;
    }

    private void ParseOption()
    {
        int i = 4;
        var option = new SaneOption
        {
            Capabilities = SaneCapabilities.SoftSelect
        };
        var optionValueList = new List<string>();
        var builder = new StringBuilder();
        _state = OptionParseState.ReadingName;
        while (i < _line!.Length)
        {
            char c = _line[i];
            switch (_state)
            {
                case OptionParseState.ReadingName:
                    if (char.IsLetter(c) || c == '-')
                    {
                        builder.Append(c);
                        i += 1;
                        break;
                    }
                    option.Name = builder.ToString();
                    builder.Clear();

                    if (c == '[')
                    {
                        option.Type = SaneValueType.Bool;
                        i += 3;
                        _state = OptionParseState.ReadingBooleanValues;
                    }
                    else if (c == ' ')
                    {
                        i += 1;
                        _state = OptionParseState.ReadingValues;
                    }
                    else if (c == '\n')
                    {
                        option.Type = SaneValueType.Button;
                        i += 1;
                    }
                    else
                    {
                        return;
                    }
                    break;

                case OptionParseState.ReadingBooleanValues:
                    if (c == 'a')
                    {
                        option.Capabilities |= SaneCapabilities.Automatic;
                        i += 1;
                    }
                    else if (c == ')')
                    {
                        i += 1;
                    }
                    else
                    {
                        i += 1;
                    }
                    break;

                case OptionParseState.ReadingValues:
                    if (char.IsLetterOrDigit(c) || c == '.' || c == '%')
                    {
                        builder.Append(c);
                        i += 1;
                        break;
                    }
                    string optionValue = builder.ToString();
                    builder.Clear();
                    if (c == ' ' || c == '\n')
                    {
                        foreach (var unitKvp in Units)
                        {
                            if (optionValue.EndsWith(unitKvp.Key, StringComparison.InvariantCultureIgnoreCase))
                            {
                                optionValue = optionValue.Substring(0, optionValue.Length - unitKvp.Key.Length);
                                option.Unit = unitKvp.Value;
                                break;
                            }
                        }
                    }
                    else if (_line.Substring(i, 4) == ",...")
                    {
                        option.Type = SaneValueType.Group;
                        i += 3;
                    }
                    else if (c != '|')
                    {
                        return;
                    }

                    if (optionValue == "auto")
                    {
                        option.Capabilities |= SaneCapabilities.Automatic;
                    }
                    else if (optionValue.Contains(".."))
                    {
                        var parts = optionValue.Split(new[] { ".." }, StringSplitOptions.None);
                        if (!decimal.TryParse(parts[0], out var min) || !decimal.TryParse(parts[1], out var max))
                        {
                            return;
                        }
                        if (option.Type == SaneValueType.None)
                        {
                            option.Type = SaneValueType.Numeric;
                        }
                        option.ConstraintType = SaneConstraintType.Range;
                        option.Range = new SaneRange
                        {
                            Min = min,
                            Max = max
                        };
                    }
                    else
                    {
                        if (option.Type == SaneValueType.None)
                        {
                            // Assume numeric until proven otherwise
                            option.Type = SaneValueType.Numeric;
                            option.ConstraintType = SaneConstraintType.WordList;
                        }
                        if (optionValue.Any(c1 => !char.IsDigit(c1) && c1 != '.'))
                        {
                            option.Type = SaneValueType.String;
                            option.ConstraintType = SaneConstraintType.StringList;
                        }
                        if (optionValue != "")
                        {
                            optionValueList.Add(optionValue);
                        }
                    }

                    if (c == ' ')
                    {
                        if (option.ConstraintType == SaneConstraintType.WordList)
                        {
                            option.WordList = optionValueList.Select(decimal.Parse).ToList();
                        }
                        if (option.ConstraintType == SaneConstraintType.StringList)
                        {
                            option.StringList = optionValueList;
                        }
                        _state = option.ConstraintType == SaneConstraintType.Range ? OptionParseState.LookingForQuant : OptionParseState.LookingForDefaultValue;
                    }
                    i += 1;
                    break;

                case OptionParseState.LookingForQuant:
                    if (c == ' ')
                    {
                        i += 1;
                        break;
                    }
                    if (c == '[')
                    {
                        _state = OptionParseState.LookingForDefaultValue;
                        break;
                    }
                    if (c == '\n')
                    {
                        i += 1;
                        break;
                    }
                    if (_line.Substring(i, 13) != "(in steps of ")
                    {
                        return;
                    }
                    i += 13;
                    _state = OptionParseState.ReadingQuant;
                    break;

                case OptionParseState.ReadingQuant:
                    if (c != ')')
                    {
                        builder.Append(c);
                        i += 1;
                        break;
                    }
                    if (!decimal.TryParse(builder.ToString(), out var quant))
                    {
                        return;
                    }
                    builder.Clear();
                    option.Range!.Quant = quant;
                    i += 1;
                    _state = OptionParseState.LookingForDefaultValue;
                    break;

                case OptionParseState.LookingForDefaultValue:
                    if (c == ' ')
                    {
                        i += 1;
                        break;
                    }
                    if (c == '\n')
                    {
                        i += 1;
                        break;
                    }
                    if (c != '[')
                    {
                        return;
                    }
                    i += 1;
                    _state = OptionParseState.ReadingDefaultValue;
                    break;

                case OptionParseState.ReadingDefaultValue:
                    if (c != ']')
                    {
                        builder.Append(c);
                        i += 1;
                        break;
                    }

                    string current = builder.ToString();
                    builder.Clear();

                    if (current == "inactive")
                    {
                        option.Capabilities |= SaneCapabilities.Inactive;
                    }
                    else if (current == "hardware")
                    {
                        option.Capabilities |= SaneCapabilities.HardSelect;
                    }
                    else if (current == "read-only")
                    {
                        option.Capabilities &= ~SaneCapabilities.SoftSelect;
                        option.Capabilities |= SaneCapabilities.SoftDetect;
                    }
                    else if (option.Type == SaneValueType.Numeric)
                    {
                        if (!decimal.TryParse(current, out var currentNumeric))
                        {
                            return;
                        }
                        option.CurrentNumericValue = currentNumeric;
                    }
                    else
                    {
                        option.CurrentStringValue = current;
                    }

                    i += 1;
                    _state = OptionParseState.Idle;
                    break;

                case OptionParseState.Idle:
                    i += 1;
                    break;
            }
        }

        _options!.Add(option);
        _lastOption = option;
        _state = OptionParseState.ReadingDescription;
    }

    private void NextLine()
    {
        _line = _input!.ReadLine();
        if (_line != null)
        {
            _line = _line.TrimEnd() + "\n";
        }
    }

    private enum OptionParseState
    {
        NonDeviceOptions,
        LookingForOption,
        ReadingDescription,
        ReadingName,
        ReadingBooleanValues,
        ReadingValues,
        LookingForQuant,
        ReadingQuant,
        LookingForDefaultValue,
        ReadingDefaultValue,
        Idle
    }
}