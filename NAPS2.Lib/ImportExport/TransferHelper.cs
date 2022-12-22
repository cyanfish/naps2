using Eto.Forms;
using Google.Protobuf;

namespace NAPS2.ImportExport;

/// <summary>
/// Generic helper for transferring data through copy/paste or drag/drop.
/// </summary>
/// <typeparam name="TInput">The domain type to be transferred.</typeparam>
/// <typeparam name="TData">The protobuf type representing the transferred data.</typeparam>
public abstract class TransferHelper<TInput, TData> where TData : IMessage<TData>, new()
{
    public string TypeName { get; } = typeof(TData).FullName!;

    /// <summary>
    /// Clears the clipboard and stores the serialized input.
    /// </summary>
    /// <param name="input"></param>
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
        dataObject.SetData(ToBinaryData(input), TypeName);
    }

    public bool IsIn(IDataObject dataObject)
    {
        return dataObject.Contains(TypeName);
    }

    public TData GetFrom(IDataObject dataObject) => FromBinaryData(dataObject.GetData(TypeName));

    /// <summary>
    /// Converts the domain type to the protobuf type for serialization.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    protected abstract TData AsData(TInput input);

    public byte[] ToBinaryData(TInput input) => AsData(input).ToByteArray();

    public TData FromBinaryData(byte[] data)
    {
        var dataObj = new TData();
        dataObj.MergeFrom(data);
        return dataObj;
    }
}