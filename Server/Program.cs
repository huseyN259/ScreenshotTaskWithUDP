using System.Net;
using System.Net.Sockets;
using System.Drawing;

var listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,ProtocolType.Udp);

var ip = IPAddress.Parse("127.0.0.1");
var port = 12345;

var ep = new IPEndPoint(ip, port);
listener.Bind(ep);

Console.WriteLine($"Server is started on {ep}");

byte[] buffer = new byte[ushort.MaxValue - 29];

var remoteEP = new IPEndPoint(IPAddress.Any, 0);

while (true)
{
	var recieve = await listener.ReceiveFromAsync(buffer, SocketFlags.None, remoteEP);
	var image = TakeScreenshot();
	var imageBytes = ImageToByte(image);
	var size = ushort.MaxValue - 29;
	var fill = imageBytes.Chunk(size);
	var newArea = fill.ToArray();

	for (int i = 0; i < newArea.Length; i++)
	{
		await Task.Delay(100);
		await listener.SendToAsync(newArea[i], SocketFlags.None, recieve.RemoteEndPoint);
	}

	Console.WriteLine($"Sent screenshot to {recieve.RemoteEndPoint}");
}

Image TakeScreenshot()
{
	Bitmap bitmap = new Bitmap(1920, 1080);

	Graphics graphics = Graphics.FromImage(bitmap);
	graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

	return bitmap;
}

byte[] ImageToByte(Image image)
{
	using (var stream = new MemoryStream())
	{
		image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
		
		return stream.ToArray();
	}
}