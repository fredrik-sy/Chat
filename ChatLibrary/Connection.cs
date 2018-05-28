using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Connection
{
    private StreamReader streamReader;
    private StreamWriter streamWriter;
    private TaskFactory taskFactory;

    public Connection(TcpClient tcpClient, TaskFactory taskFactory)
    {
        TcpClient = tcpClient;
        Stream stream = tcpClient.GetStream();
        streamReader = new StreamReader(stream);
        streamWriter = new StreamWriter(stream);
        this.taskFactory = taskFactory;
    }

    public TcpClient TcpClient { get; }

    /// <summary>
    /// Closes this <see cref="Connection"/> instance.
    /// </summary>
    public void Close()
    {
        streamReader.Close();
        streamWriter.Close();
        TcpClient.Close();
    }

    /// <summary>
    /// Reads a line asynchronously from the stream.
    /// </summary>
    public Task<string> ReadLineAsync()
    {
        return taskFactory.StartNew(streamReader.ReadLine);
    }

    /// <summary>
    /// Writes a line to the stream.
    /// </summary>
    public void WriteLine(string value)
    {
        streamWriter.WriteLine(value);
        streamWriter.Flush();
    }
}
