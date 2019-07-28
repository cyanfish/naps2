using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Images;

namespace NAPS2.WinForms
{
    public class SelectableListView<T> : ISelectable<T>
    {
        private readonly ListView listView;
        private ListSelection<T> selection = ListSelection.Empty<T>();
        private bool refreshing;

        public SelectableListView(ListView listView)
        {
            this.listView = listView;
            listView.SelectedIndexChanged += ListViewOnSelectedIndexChanged;
        }

        public event EventHandler SelectionChanged;

        private void ListViewOnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!refreshing)
            {
                refreshing = true;
                Selection = ListSelection.From(listView.SelectedItems.Cast<ListViewItem>().Select(x => (T) x.Tag));
                refreshing = false;
            }
        }

        public ListSelection<T> Selection
        {
            get => selection;
            set
            {
                selection = value ?? throw new ArgumentNullException(nameof(value));
                if (!refreshing)
                {
                    refreshing = true;
                    for (int i = 0; i < listView.Items.Count; i++)
                    {
                        listView.Items[i].Selected = selection.Contains((T) listView.Items[i].Tag);
                    }
                    refreshing = false;
                }
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RefreshItems(IEnumerable<T> items, Func<T, string> labelFunc, Func<T, int> imageIndexFunc)
        {
            refreshing = true;
            listView.Items.Clear();
            foreach (var item in items)
            {
                var listViewItem = listView.Items.Add(labelFunc(item), imageIndexFunc(item));
                listViewItem.Tag = item;
            }
            for (int i = 0; i < listView.Items.Count; i++)
            {
                listView.Items[i].Selected = Selection.Contains((T) listView.Items[i].Tag);
            }
            refreshing = false;
        }
    }
}
