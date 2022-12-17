namespace NAPS2.Escl.Client;

public record EsclUsbDescriptor(int VendorId, int ProductId, string SerialNumber, string Manufacturer, string Product);