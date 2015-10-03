using Media.Rtsp.Server.MediaTypes;

namespace Salsa20.Stream
{
    public class StreamServer
    {
        public void Start()
        {
            //Create the server optionally specifying the port to listen on
            using (var server = new Media.Rtsp.RtspServer())
            {

                //Create a stream which will be exposed under the name Uri rtsp://localhost/live/RtspSourceTest
                //From the RtspSource rtsp://1.2.3.4/mpeg4/media.amp

                //var source = new RtspSource("RtspSourceTest", "rtsp://1.2.3.4/mpeg4/media.amp");
                var source = new RtspSource("RtspSourceTest", "rtsp://1.2.3.4/mpeg4/media.amp");

                //If the stream had a username and password
                //source.Client.Credential = new System.Net.NetworkCredential("user", "password");

                //If you wanted to password protect the stream
                //source.RtspCredential = new System.Net.NetworkCredential("username", "password");
                server.TryAddMedia(source);
                //Add the stream to the server
                
                //Start the server and underlying streams
                server.Start();
            }
        }
    }
}
