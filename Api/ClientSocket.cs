using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using YaMusicRPC;

public class ClientSocket
{
    private Socket client;

    public void Connect(string server, int port)
    {
        try
        {
            // Устанавливаем сокет-клиент
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Подключаемся к серверу
            client.Connect(server, port);
            Debug.WriteLine("Connected to server {0}:{1}", server, port);
        }
        catch (Exception ex)
        {
            ErrorLogger logger = new ErrorLogger("error.log");
            logger.LogError(ex);
        }
    }

    public void Disconnect()
    {
        try
        {
            // Закрываем соединение с сервером
            client.Shutdown(SocketShutdown.Both);
            client.Close();
            Debug.WriteLine("Disconnected from server.");
        }
        catch (Exception ex)
        {
            ErrorLogger logger = new ErrorLogger("error.log");
            logger.LogError(ex);
        }
    }

    public void Send(string progress,string track,string plus)
    {
        try
        {
            string data = progress + "|" + track + "|" + plus;
            // Преобразуем строку в байты и отправляем данные на сервер
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            //Debug.WriteLine("Data: " + data);
            client.Send(byteData);
        }
        catch (Exception ex)
        {
            ErrorLogger logger = new ErrorLogger("error.log");
            logger.LogError(ex);
        }
    }

    internal void Clear()
    {
        try
        {
            string data = "ClearRPC";
            // Преобразуем строку в байты и отправляем данные на сервер
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            Debug.WriteLine(data);
            client.Send(byteData);
        }
        catch (Exception ex)
        {
            ErrorLogger logger = new ErrorLogger("error.log");
            logger.LogError(ex);
        }
    }
}
