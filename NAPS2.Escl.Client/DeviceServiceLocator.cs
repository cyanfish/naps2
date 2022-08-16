using Zeroconf;

namespace NAPS2.Escl.Client;

public class DeviceServiceLocator
{
    public async Task Something()
    {
        // TODO: ResolveContinuous is also an option
        var results = await ZeroconfResolver.ResolveAsync(new[]
        {
            "_uscan._tcp",
            "_uscans._tcp"
        });
        foreach (var r in results)
        {
            var ip = r.IPAddress;
            foreach (var s in r.Services.Values)
            {
                var port = s.Port;
                var name = s.Name;
                var serviceName = s.ServiceName;
                // TODO: Merge properties from multiple sets?
                var props = s.Properties.FirstOrDefault();
                if (props != null)
                {
                    // var txtVers = props.GetValueOrDefault("txtvers"); // txt record version
                    // var adminUrl = props.GetValueOrDefault("adminurl"); // url to scanner config page
                    // var esclVersion = props.GetValueOrDefault("Vers"); // escl version e.g. "2.0"
                    // var thumbnail = props.GetValueOrDefault("representation"); // url to png or ico 
                    // var urlBasePath = props.GetValueOrDefault("rs"); // no leading (or trailing) slash
                    // var scannerName = props.GetValueOrDefault("ty"); // human readable
                    // var note = props.GetValueOrDefault("note"); // supposed to be "scanner location", e.g. "Copy Room"
                    // // Note jpeg is better in that we can get one image at a time, but pdf does allow png quality potentially
                    // // Hopefully decent scanners can support png too
                    // // Also for the server we can definitely provide NAPS2-generated pdfs, which is kind of a cool idea for e.g. using from mobile
                    // var pdl = props.GetValueOrDefault("pdl"); // comma separated mime types supported "application/pdf,image/jpeg" at minimum
                    // var uuid = props.GetValueOrDefault("uuid"); // physical device id
                    // var colorSpace = props.GetValueOrDefault("cs"); // comma separated capabilites, "color,grayscale,binary"
                    // var source = props.GetValueOrDefault("is"); // "platen,adf,camera" platen = flatbed
                    // var duplex = props.GetValueOrDefault("duplex"); // "T"rue or "F"alse
                    //
                }
            }
        }
    }
}