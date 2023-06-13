using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main()
    {
        // Endereço IP local e porta TCP
        IPAddress ipAddress = IPAddress.Any;
        int port = 1908;

        // Cria o objeto TcpListener
        TcpListener listener = new TcpListener(ipAddress, port);

        // Inicia o TcpListener
        listener.Start();
        Console.WriteLine("Gestão do Monitor Vertical. Ouvindo porta {0}...", port);

        try
        {
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                Console.WriteLine("Cliente conectado!");

                // Cria uma tarefa assíncrona para processar o cliente.
                // Foi necesário para paralelizar a chamada do ps
                Task.Run(async () =>
                {
                    await ProcessClient(client);
                   // await Task.Delay(2000); 
                    client.Close();
                    Console.WriteLine("Cliente desconectado!");
                });

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro: {0}", ex.Message);
        }
        finally
        {
            // Finaliza o TcpListener
            listener.Stop();
        }
    }

    static async Task ProcessClient(TcpClient client)
    {
       
        // Obtém a stream de leitura e escrita do cliente
        NetworkStream stream = client.GetStream();

        // Lê os dados enviados pelo cliente
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);

        // Exibe os dados recebidos
        if(dataReceived == "iniciar")
        {
            Console.WriteLine("Iniciado processo Xibo");
        }else if (dataReceived == "parar")
        {
            Console.WriteLine("Parado o processo Xibo");
        }


        // Verifica se a mensagem recebida é "iniciar", "parar"
        if (dataReceived.Trim().ToLower() == "iniciar")
        {
            // Chama o script PowerShell
            string scriptPath = "C:\\XiboVertical\\iniciar.ps1";
            ExecutePowerShellScript(scriptPath);
        }
        else if (dataReceived.Trim().ToLower() == "parar")
        {
            // Chama o script PowerShell
            string scriptPath = "C:\\XiboVertical\\parar.ps1";
            ExecutePowerShellScript(scriptPath);
        }
        else if (dataReceived.Trim().ToLower() == "reiniciar")
        {
            // Parar
            string scriptPath = "C:\\XiboVertical\\parar.ps1";
            ExecutePowerShellScript(scriptPath);
            // Iniciar
            string scriptPath1 = "C:\\XiboVertical\\iniciar.ps1";
            ExecutePowerShellScript(scriptPath1);
        }
        else
        {
            // Envia uma resposta para o cliente informando que a mensagem não foi reconhecida
            string response = "Mensagem não reconhecida!";
            byte[] responseData = Encoding.ASCII.GetBytes(response);
            stream.Write(responseData, 0, responseData.Length);
            Console.WriteLine("Resposta enviada para o cliente: {0}", response);
        }
     
    }

    static void ExecutePowerShellScript(string scriptPath)
    {
        // Verifica se o arquivo do script existe
        if (!File.Exists(scriptPath))
        {
            Console.WriteLine("O script não foi encontrado: {0}", scriptPath);
            return;
        }

        // Cria o objeto ProcessStartInfo para configurar a execução do script PowerShell
        var startInfo = new System.Diagnostics.ProcessStartInfo()
        {
            FileName = "powershell.exe",
            Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Inicia o processo do PowerShell
        var process = new System.Diagnostics.Process()
        {
            StartInfo = startInfo
        };

        // Inicia a execução do script
        process.Start();

        // Lê a saída do processo (output e error)
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        // Aguarda o término do processo
        process.WaitForExit();

        // Exibe a saída e o erro do script
        //Console.WriteLine("Saída do script:");
        //Console.WriteLine(output);
        //Console.WriteLine("Erro do script:");
        //Console.WriteLine(error);
    }
}
