using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NAPS2.Scan.Sane
{
    public class SaneOptionParser
    {
        private StreamReader input;
        private SaneOptionCollection options;
        private SaneOption lastOption;
        private OptionParseState state;
        private string line;

        public SaneOptionCollection Parse(StreamReader streamReader)
        {
            input = streamReader;
            options = new SaneOptionCollection();
            state = OptionParseState.NonDeviceOptions;
            line = null;

            NextLine();
            while (line != null)
            {
                switch (state)
                {
                    case OptionParseState.NonDeviceOptions:
                        if (line.StartsWith("Options specific to device", StringComparison.InvariantCultureIgnoreCase))
                        {
                            state = OptionParseState.LookingForOption;
                        }
                        NextLine();
                        break;

                    case OptionParseState.LookingForOption:
                        if (line.StartsWith("    -", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ParseOption();
                            state = OptionParseState.ReadingDescription;
                        }
                        NextLine();
                        break;

                    case OptionParseState.ReadingDescription:
                        if (line.StartsWith("        ", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string descLine = line.Substring(8);
                            if (lastOption.Desc == null)
                            {
                                lastOption.Desc = descLine;
                            }
                            else
                            {
                                lastOption.Desc += " " + descLine;
                            }
                            NextLine();
                        }
                        else
                        {
                            state = OptionParseState.LookingForOption;
                        }
                        break;
                }
            }

            return options;
        }

        private void ParseOption()
        {
            int i = 4;
            var option = new SaneOption();
            var builder = new StringBuilder();
            state = OptionParseState.ReadingName;
            while (i < line.Length)
            {
                char c = line[i];
                switch (state)
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
                            state = OptionParseState.ReadingBooleanValues;
                        }
                        else if (c == ' ')
                        {
                            i += 1;
                            state = OptionParseState.ReadingValues;
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
                            option.Capabilitieses |= SaneCapabilities.Automatic;
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
                        // TODO
                        i += 1;
                        break;
                }
            }

            options.Add(option);
            lastOption = option;
            state = OptionParseState.ReadingDescription;
        }

        private void NextLine()
        {
            line = input.ReadLine();
            if (line != null)
            {
                line = line.TrimEnd() + "\n";
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
            ReadingDefaultValue
        }
    }
}
