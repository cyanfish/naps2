using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    /// <summary>
    /// A layout manager for WinForms that takes advantage of the relative sizes and positions of controls from the WinForms designer.
    /// When the LayoutManager is activated, the layout will initially stay the same and then smoothly change as the form is resized.
    /// Note that the layout manager is designed to be "stretch-only", i.e. it won't change the flow of the controls.
    /// 
    /// LayoutManager provides fluent syntax that is typically used similarly to the following:
    ///     new LayoutManager(this)                 // Init layout manager with a reference to the form
    ///         .Bind(control1)                     // Start describing bindings for control1
    ///             .WidthToForm()                  // control1's width should change with the form's width
    ///         .Bind(control1, control2)           // Start describing bindings for control1 AND control2
    ///             .Right(() => control3.Right)    // Both control1 and control2's right sides should stay aligned with control3's right side
    ///             .BottomToForm()                 // Both control1 and control2's bottom side should stay aligned with the form's bottom side
    ///         .Activate();                        // Start laying out controls in response to Resize events
    /// 
    /// Note that the following bindings (for example) are equivalent:
    ///     .LeftToForm()
    ///     .LeftTo(() => form.Left)
    /// 
    /// The more general form of binding (e.g. LeftTo) can be used with arbitrary expressions.
    /// Note that dependency tracking will only work if the dependent controls are accessed in the expression.
    /// Examples:
    ///     .WidthTo(() => form.Width / 2)                                  // OK
    ///     .LeftTo(() => control1.Left + control2.Width + SomeFunction())  // OK if SomeFunction doesn't depend on the size or position of any controls other than control1 or control2
    ///     .LeftTo(() => SomeFunction(control1))                           // OK
    ///     .LeftTo(() => SomeFunctionDependingOnControl1())                // NOT OK
    /// </summary>
    public class LayoutManager
    {
        /// <summary>
        /// A list of controls and their bindings at the time of activation, in a topological order
        /// </summary>
        private readonly List<KeyValuePair<Control, HashSet<Binding>>> activatedControlBindings = new List<KeyValuePair<Control, HashSet<Binding>>>();

        /// <summary>
        /// Initializes an instance of the LayoutManager class that will layout the controls of the given form.
        /// </summary>
        /// <param name="form">The form whose controls will be laid out.</param>
        public LayoutManager(Form form)
        {
            Form = form;
            Bindings = new List<Binding>();
        }

        /// <summary>
        /// Gets the form whose layout the LayoutManager is managing.
        /// </summary>
        public Form Form { get; }

        /// <summary>
        /// Gets a list of bindings belonging to the LayoutManager.
        /// Changes to this list are applied at activation.
        /// </summary>
        public List<Binding> Bindings { get; }

        /// <summary>
        /// Gets a value indicating whether the LayoutManager has been activated (but not deacivated since).
        /// </summary>
        public bool Activated { get; private set; }

        /// <summary>
        /// Begins the description of a set of bindings for the given controls.
        /// </summary>
        /// <param name="controls">The controls to which to apply the following bindings.</param>
        /// <returns>An object that provides an interface to describe bindings.</returns>
        public BindingSyntax Bind(params Control[] controls)
        {
            return new BindingSyntax(this, controls);
        }

        /// <summary>
        /// Activates the LayoutManager. This performs dependency analysis and starts listening for the form's Resize event (upon which the layout will be updated).
        /// The relative positions of bound controls is determined by the offset at activation.
        /// For example, if a control's right side is bound to the form's right side and its right side is 12 pixels to the
        /// left of the form's right side, that 12 pixel difference will be maintained by the LayoutManager.
        /// </summary>
        public void Activate()
        {
            if (!Activated)
            {
                Form.Resize += OnFormResize;
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

        /// <summary>
        /// Deactivates the LayoutManager. Stops listening for the form's Resize event.
        /// </summary>
        public void Deactivate()
        {
            if (Activated)
            {
                Form.Resize -= OnFormResize;
                Activated = false;
            }
        }

        private void OnFormResize(object sender, EventArgs args)
        {
            UpdateLayout();
        }

        /// <summary>
        /// Immediately updates the layout of bound controls. This is automatically triggered by the form's Resize event if the LayoutManager is activated.
        /// </summary>
        public void UpdateLayout()
        {
            if (!Activated)
            {
                throw new InvalidOperationException("The LayoutManager can't update the layout without being activated first.");
            }
            foreach (var pair in activatedControlBindings)
            {
                var control = pair.Key;
                var bindings = pair.Value;
                var bindingTypesUsed = new bool[(int)BindingType.Bottom + 1];
                foreach (var binding in bindings.OrderBy(x => x.BindingType == BindingType.Right || x.BindingType == BindingType.Bottom ? 1 : 0))
                {
                    if (bindingTypesUsed[(int)binding.BindingType])
                    {
                        throw new InvalidOperationException("Duplicate " + binding.BindingType + " binding for control " + control.Name + ".");
                    }
                    bindingTypesUsed[(int)binding.BindingType] = true;

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
                                throw new InvalidOperationException("The layout bindings (Left/Right/Width) for control " + control.Name + " are overspecified.");
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
                                throw new InvalidOperationException("The layout bindings (Top/Bottom/Height) for control " + control.Name + " are overspecified.");
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

        private class DependencyWalker : ExpressionVisitor
        {
            public DependencyWalker()
            {
                Dependencies = new HashSet<Control>();
            }

            public HashSet<Control> Dependencies { get; }

            protected override Expression VisitMember(MemberExpression node)
            {
                AddDeps(node.Expression);
                AddDeps(node);
                return base.VisitMember(node);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                AddDeps(node);
                return base.VisitConstant(node);
            }

            private void AddDeps(Expression expr)
            {
                if (typeof(Control).IsAssignableFrom(expr.Type))
                {
                    Dependencies.Add(
                        ((Expression<Func<Control>>)Expression.Lambda(Expression.Convert(expr, typeof(Control))))
                            .Compile()());
                }
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

            public Control Control { get; }

            public BindingType BindingType { get; }

            public Expression<Func<int>> ValueFunc { get; }

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

            internal int DependentValue => compiledValueFunc();
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

            /// <summary>
            /// Begins the description of a new set of bindings for the given controls, and ends the previous description of bindings.
            /// </summary>
            /// <param name="controls">The controls to which to apply the following bindings.</param>
            /// <returns>An object that provides an interface to describe bindings.</returns>
            public BindingSyntax Bind(params Control[] controls)
            {
                return new BindingSyntax(layoutManager, controls);
            }

            /// <summary>
            /// Activates the LayoutManager. This performs dependency analysis and starts listening for the form's Resize event.
            /// The relative positions of bound controls is determined by the offset at activation.
            /// For example, if a control's right side is bound to the form's right side and its right side is 12 pixels to the
            /// left of the form's right side, that 12 pixel difference will be maintained by the LayoutManager.
            /// </summary>
            public LayoutManager Activate()
            {
                layoutManager.Activate();
                return layoutManager;
            }

            /// <summary>
            /// Binds the width of each of the controls in the current set to the value of the specified expression.
            /// </summary>
            /// <param name="func">The expression whose value will be bound to the controls' property.</param>
            /// <returns>An object that provides an interface to describe bindings.</returns>
            public BindingSyntax WidthTo(Expression<Func<int>> func)
            {
                foreach (var control in controls)
                {
                    layoutManager.Bindings.Add(new Binding(control, BindingType.Width, func));
                }
                return this;
            }

            /// <summary>
            /// Binds the height of each of the controls in the current set to the value of the specified expression.
            /// </summary>
            /// <param name="func">The expression whose value will be bound to the controls' property.</param>
            /// <returns>An object that provides an interface to describe bindings.</returns>
            public BindingSyntax HeightTo(Expression<Func<int>> func)
            {
                foreach (var control in controls)
                {
                    layoutManager.Bindings.Add(new Binding(control, BindingType.Height, func));
                }
                return this;
            }

            /// <summary>
            /// Binds the left side of each of the controls in the current set to the value of the specified expression.
            /// </summary>
            /// <param name="func">The expression whose value will be bound to the controls' property.</param>
            /// <returns>An object that provides an interface to describe bindings.</returns>
            public BindingSyntax LeftTo(Expression<Func<int>> func)
            {
                foreach (var control in controls)
                {
                    layoutManager.Bindings.Add(new Binding(control, BindingType.Left, func));
                }
                return this;
            }

            /// <summary>
            /// Binds the right side of each of the controls in the current set to the value of the specified expression.
            /// </summary>
            /// <param name="func">The expression whose value will be bound to the controls' property.</param>
            /// <returns>An object that provides an interface to describe bindings.</returns>
            public BindingSyntax RightTo(Expression<Func<int>> func)
            {
                foreach (var control in controls)
                {
                    layoutManager.Bindings.Add(new Binding(control, BindingType.Right, func));
                }
                return this;
            }

            /// <summary>
            /// Binds the top side of each of the controls in the current set to the value of the specified expression.
            /// </summary>
            /// <param name="func">The expression whose value will be bound to the controls' property.</param>
            /// <returns>An object that provides an interface to describe bindings.</returns>
            public BindingSyntax TopTo(Expression<Func<int>> func)
            {
                foreach (var control in controls)
                {
                    layoutManager.Bindings.Add(new Binding(control, BindingType.Top, func));
                }
                return this;
            }

            /// <summary>
            /// Binds the bottom side of each of the controls in the current set to the value of the specified expression.
            /// </summary>
            /// <param name="func">The expression whose value will be bound to the controls' property.</param>
            /// <returns>An object that provides an interface to describe bindings.</returns>
            public BindingSyntax BottomTo(Expression<Func<int>> func)
            {
                foreach (var control in controls)
                {
                    layoutManager.Bindings.Add(new Binding(control, BindingType.Bottom, func));
                }
                return this;
            }

            /// <summary>
            /// Binds the width of each of the controls in the current set to the width of the form.
            /// </summary>
            /// <returns>An object that provides an interface to describe bindings.</returns>
            public BindingSyntax WidthToForm()
            {
                return WidthTo(() => layoutManager.Form.Width);
            }

            /// <summary>
            /// Binds the height of each of the controls in the current set to the height of the form.
            /// </summary>
            /// <returns>An object that provides an interface to describe bindings.</returns>
            public BindingSyntax HeightToForm()
            {
                return HeightTo(() => layoutManager.Form.Height);
            }

            /// <summary>
            /// Binds the left side of each of the controls in the current set to the left side of the form.
            /// </summary>
            /// <returns>An object that provides an interface to describe bindings.</returns>
            public BindingSyntax LeftToForm()
            {
                return LeftTo(() => 0);
            }

            /// <summary>
            /// Binds the right side of each of the controls in the current set to the right side of the form.
            /// </summary>
            /// <returns>An object that provides an interface to describe bindings.</returns>
            public BindingSyntax RightToForm()
            {
                return RightTo(() => layoutManager.Form.Width);
            }

            /// <summary>
            /// Binds the top side of each of the controls in the current set to the top side of the form.
            /// </summary>
            /// <returns>An object that provides an interface to describe bindings.</returns>
            public BindingSyntax TopToForm()
            {
                return TopTo(() => 0);
            }

            /// <summary>
            /// Binds the bottom side of each of the controls in the current set to the bottom side of the form.
            /// </summary>
            /// <returns>An object that provides an interface to describe bindings.</returns>
            public BindingSyntax BottomToForm()
            {
                return BottomTo(() => layoutManager.Form.Height);
            }
        }
    }
}
