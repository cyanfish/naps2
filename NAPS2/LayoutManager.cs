using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace NAPS2
{
    public class LayoutManager
    {
        private readonly List<KeyValuePair<Control, HashSet<Binding>>> activatedControlBindings = new List<KeyValuePair<Control, HashSet<Binding>>>();

        public LayoutManager(Form form)
        {
            Form = form;
            Bindings = new List<Binding>();
        }

        public Form Form { get; private set; }

        public List<Binding> Bindings { get; private set; }

        public bool Activated { get; private set; }

        public BindingSyntax Bind(params Control[] controls)
        {
            return new BindingSyntax(this, controls);
        }

        public void Activate()
        {
            if (!Activated)
            {
                Form.Resize += FormResize;
                Activated = true;

                // Prepare a dependency graph for the controls 
                var controls = new HashSet<Control>(); // Nodes
                var controlBindings = new Dictionary<Control, HashSet<Binding>>(); // Node values
                var dependencies = new Dictionary<Control, HashSet<Control>>(); // Edges (direction 1)
                var dependers = new Dictionary<Control, HashSet<Control>>(); // Edges (direction 2)

                foreach (Binding binding in Bindings)
                {
                    // Set the initial values for use when evaluating the bindings
                    binding.InitialValue = binding.Value;
                    binding.InitialDependentValue = binding.DependentValue;

                    // Populate the graph nodes
                    controls.Add(binding.Control);
                    controlBindings.AddMulti(binding.Control, binding);

                    // Ensure each control has a key in dependencies/dependers
                    dependencies.AddMulti(binding.Control, Enumerable.Empty<Control>());
                    dependers.AddMulti(binding.Control, Enumerable.Empty<Control>());

                    // Populate the graph edges by examining the binding's value expression
                    var walker = new DependencyWalker();
                    walker.Visit(binding.ValueFunc);
                    foreach (Control control in walker.Dependencies)
                    {
                        if (control == binding.Control)
                        {
                            throw new InvalidOperationException("The layout bindings for control " + control.Name + " are self-dependent.");
                        }
                        controls.Add(control); // Populate the graph nodes
                        dependers.AddMulti(control, binding.Control);
                        dependencies.AddMulti(binding.Control, control);
                        dependencies.AddMulti(control, Enumerable.Empty<Control>()); // Ensure each control has a key in dependencies/dependers
                    }
                }

                // Topological sorting (see http://en.wikipedia.org/wiki/Topological_sorting)
                activatedControlBindings.Clear();
                var controlsWithoutDependencies = new HashSet<Control>(controls.Where(x => dependencies[x].Count == 0));
                while (controlsWithoutDependencies.Count > 0)
                {
                    var control = controlsWithoutDependencies.First();
                    controlsWithoutDependencies.Remove(control);
                    if (controlBindings.ContainsKey(control))
                    {
                        activatedControlBindings.Add(new KeyValuePair<Control, HashSet<Binding>>(control, controlBindings[control]));
                    }
                    while (dependers[control].Count > 0)
                    {
                        var depender = dependers[control].First();
                        dependers[control].Remove(depender);
                        dependencies[depender].Remove(control);
                        if (dependencies[depender].Count == 0)
                        {
                            controlsWithoutDependencies.Add(depender);
                        }
                    }
                }

                var controlsInDependencyCycles = dependencies.Where(x => x.Value.Count > 0).ToList();
                if (controlsInDependencyCycles.Count > 0)
                {
                    throw new InvalidOperationException("The layout bindings have one or more cycles including these controls: " + string.Join(", ", controlsInDependencyCycles.Select(x => x.Key.Name)));
                }
            }
        }

        public void Deactivate()
        {
            if (Activated)
            {
                Form.Resize -= FormResize;
                Activated = false;
            }
        }

        private void FormResize(object sender, EventArgs eventArgs)
        {
            foreach (var pair in activatedControlBindings)
            {
                var control = pair.Key;
                var bindings = pair.Value;
                foreach (var binding in bindings.OrderBy(x => x.BindingType == BindingType.Right || x.BindingType == BindingType.Bottom ? 1 : 0))
                {
                    int offset = binding.InitialValue - binding.InitialDependentValue;
                    switch (binding.BindingType)
                    {
                        case BindingType.Width:
                        case BindingType.Height:
                        case BindingType.Left:
                        case BindingType.Top:
                            binding.Value = binding.DependentValue + offset;
                            break;
                        case BindingType.Right:
                            bool hasWidthBinding = bindings.Any(x => x.BindingType == BindingType.Width);
                            bool hasLeftBinding = bindings.Any(x => x.BindingType == BindingType.Left);
                            if (hasWidthBinding && hasLeftBinding)
                            {
                                throw new InvalidOperationException("The layout bindings for control " + control.Name + " are overspecified.");
                            }
                            else if (hasLeftBinding)
                            {
                                control.Width = binding.DependentValue + offset - control.Left;
                            }
                            else
                            {
                                control.Left = binding.DependentValue + offset - control.Width;
                            }
                            break;
                        case BindingType.Bottom:
                            bool hasHeightBinding = bindings.Any(x => x.BindingType == BindingType.Height);
                            bool hasTopBinding = bindings.Any(x => x.BindingType == BindingType.Top);
                            if (hasHeightBinding && hasTopBinding)
                            {
                                throw new InvalidOperationException("The layout bindings for control " + control.Name + " are overspecified.");
                            }
                            else if (hasTopBinding)
                            {
                                control.Height = binding.DependentValue + offset - control.Top;
                            }
                            else
                            {
                                control.Top = binding.DependentValue + offset - control.Height;
                            }
                            break;
                    }
                }
            }
        }

        public class DependencyWalker : ExpressionVisitor
        {
            public DependencyWalker()
            {
                Dependencies = new List<Control>();
            }

            public List<Control> Dependencies { get; private set; }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (typeof(Control).IsAssignableFrom(node.Expression.Type))
                {
                    Dependencies.Add(((Expression<Func<Control>>)Expression.Lambda(Expression.Convert(node.Expression, typeof(Control)))).Compile()());
                }
                return base.VisitMember(node);
            }
        }

        public class Binding
        {
            private readonly Func<int> compiledValueFunc;

            public Binding(Control control, BindingType bindingType, Expression<Func<int>> valueFunc)
            {
                Control = control;
                BindingType = bindingType;
                ValueFunc = valueFunc;
                compiledValueFunc = valueFunc.Compile();
            }

            public Control Control { get; private set; }

            public BindingType BindingType { get; private set; }

            public Expression<Func<int>> ValueFunc { get; private set; }

            internal int InitialValue { get; set; }

            internal int InitialDependentValue { get; set; }

            internal int Value
            {
                get
                {
                    switch (BindingType)
                    {
                        case BindingType.Width:
                            return Control.Width;
                        case BindingType.Height:
                            return Control.Height;
                        case BindingType.Left:
                            return Control.Left;
                        case BindingType.Right:
                            return Control.Right;
                        case BindingType.Top:
                            return Control.Top;
                        case BindingType.Bottom:
                            return Control.Bottom;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                set
                {
                    switch (BindingType)
                    {
                        case BindingType.Width:
                            Control.Width = value;
                            break;
                        case BindingType.Height:
                            Control.Height = value;
                            break;
                        case BindingType.Left:
                            Control.Left = value;
                            break;
                        case BindingType.Top:
                            Control.Top = value;
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            internal int DependentValue
            {
                get
                {
                    return compiledValueFunc();
                }
            }
        }

        public enum BindingType
        {
            Width,
            Height,
            Left,
            Right,
            Top,
            Bottom
        }

        public class BindingSyntax
        {
            private readonly LayoutManager layoutManager;
            private readonly Control[] controls;

            public BindingSyntax(LayoutManager layoutManager, Control[] controls)
            {
                this.layoutManager = layoutManager;
                this.controls = controls;
            }

            public BindingSyntax Bind(params Control[] controls)
            {
                return new BindingSyntax(layoutManager, controls);
            }

            public LayoutManager Activate()
            {
                layoutManager.Activate();
                return layoutManager;
            }

            public BindingSyntax Width(Expression<Func<int>> widthFunc)
            {
                foreach (var control in controls)
                {
                    layoutManager.Bindings.Add(new Binding(control, BindingType.Width, widthFunc));
                }
                return this;
            }

            public BindingSyntax Height(Expression<Func<int>> widthFunc)
            {
                foreach (var control in controls)
                {
                    layoutManager.Bindings.Add(new Binding(control, BindingType.Height, widthFunc));
                }
                return this;
            }

            public BindingSyntax Left(Expression<Func<int>> widthFunc)
            {
                foreach (var control in controls)
                {
                    layoutManager.Bindings.Add(new Binding(control, BindingType.Left, widthFunc));
                }
                return this;
            }

            public BindingSyntax Right(Expression<Func<int>> widthFunc)
            {
                foreach (var control in controls)
                {
                    layoutManager.Bindings.Add(new Binding(control, BindingType.Right, widthFunc));
                }
                return this;
            }

            public BindingSyntax Top(Expression<Func<int>> widthFunc)
            {
                foreach (var control in controls)
                {
                    layoutManager.Bindings.Add(new Binding(control, BindingType.Top, widthFunc));
                }
                return this;
            }

            public BindingSyntax Bottom(Expression<Func<int>> widthFunc)
            {
                foreach (var control in controls)
                {
                    layoutManager.Bindings.Add(new Binding(control, BindingType.Bottom, widthFunc));
                }
                return this;
            }
        }
    }
}
