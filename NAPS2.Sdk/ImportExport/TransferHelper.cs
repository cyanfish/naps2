using Eto.Forms;
using Google.Protobuf;

namespace NAPS2.ImportExport
{
    public abstract class TransferHelper<TInput, TData> where TData : IMessage<TData>, new()
    {
        private readonly string _typeName = typeof(TData).FullName;

        public void SetClipboard(TInput input)
        {
            Clipboard.Instance.Clear();
            AddToClipboard(input);
        }

        public void AddToClipboard(TInput input) => AddTo(Clipboard.Instance, input);
        public bool IsInClipboard() => IsIn(Clipboard.Instance);
        public TData GetFromClipboard() => GetFrom(Clipboard.Instance);

        public void AddTo(IDataObject dataObject, TInput input)
        {
            dataObject.SetData(AsData(input).ToByteArray(), _typeName);
        }

        public bool IsIn(IDataObject dataObject)
        {
            return dataObject.Contains(_typeName);
        }

        public TData GetFrom(IDataObject dataObject)
        {
            var data = new TData();
            data.MergeFrom(dataObject.GetData(_typeName));
            return data;
        }

        protected abstract TData AsData(TInput input);
    }
}
