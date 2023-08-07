using System.IO;
using System.Runtime.ConstrainedExecution;
using System;
using HuffmanComp;
using System.Collections;

namespace HuffmanComp;

public class Program
{
    // MAIN
    static void Main(string[] args)
    {
        // Arquivo a ser manipulado
        byte[] file = new byte[0]; // Stream
        string? filePath; // Caminho do arquivo

        try
        {
            Console.WriteLine("Seja bem vindo!!!");
            Console.Write("Por favor, informe o caminho do arquivo a ser manipulado: ");
            filePath = Console.ReadLine();

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                file = File.ReadAllBytes(filePath); // Abre o arquivo

                Console.Write("Digite 'C' caso deseje comprimir seu arquivo ou 'D' para descomprimi-lo: ");
                var compOrDesc = (char)Console.Read();
                while (char.ToUpper(compOrDesc) != 'C' && char.ToUpper(compOrDesc) != 'D')
                {
                    Console.Write("Entrada invalida, utilize 'C' caso deseje comprimir seu arquivo ou 'D' para descomprimi-lo: ");
                    compOrDesc = (char)Console.Read();
                }

                // Comprimindo arquivo
                if (char.ToUpper(compOrDesc) == 'C')
                {

                    //Gera Lista de frequencias de caracteres para ordenar arvore binaria
                    var freqList = new List<BinaryTreeNode>();
                    var bytesList = new List<byte>(file);

                    // Pega o primeiro item da lista de caracteres, pega o numero de vezes que ele e aparece
                    // e remove todas as aparicoes da lista apos gerar um no da arvore binaria com as informacoes
                    while (bytesList.Count() > 0)
                    {
                        var cur = bytesList[0];
                        var count = bytesList.Count(x => x == cur);
                        freqList.Add(new BinaryTreeNode(count, cur));
                        bytesList.RemoveAll(x => x == cur);
                    }

                    SortList(ref freqList);

                    //Remove os primeiros nos da lista e gera um no pai com a soma das frequencias
                    //Iteracao acaba quando so sobrar o no inicial da arvore na lista de frequencias
                    while(freqList.Count() > 1)
                    {
                        var node1 = freqList[0];
                        var node2 = freqList[1];
                        freqList.RemoveRange(0, 2);

                        var newNode = new BinaryTreeNode(node1.Freq + node1.Freq);
                        newNode.Left = node1;
                        newNode.Right = node2;

                        if (freqList.Count() == 0)
                        {
                            freqList.Add(newNode);
                        }

                        else
                        {
                            var last = freqList.FindLastIndex(0, 1, x => x.Freq <= newNode.Freq) ;
                            freqList.Insert(last < 1 ? 0 : last , newNode);
                        }
                    }

                    // Define a tabela de valores para cada caracter
                    var dict = new Dictionary<byte, BitArray>();
                    getLeafs(ref dict, freqList.First());


                    // Cria novo arquivo para armazenar os dados compactados
                    var filePathSplitted = filePath.Split(@"\");
                    filePathSplitted[filePathSplitted.GetUpperBound(0)] = filePathSplitted[filePathSplitted.GetUpperBound(0)].Replace(".txt", "-COMP.txt");
                    var newFile = File.Create(string.Join(@"\", filePathSplitted));

                    //Gera um array de byte para criacao do novo arquivo compactado
                    var curByte = new byte[1];
                    var body = new List<byte>();
                    var bitArray = new BitArray(8);
                    var cont = 0;

                    for (int i = 0; i < file.Length; i++)
                    {
                        var bits = dict.GetValueOrDefault(file[i]);
                        for(int j = 0; j < bits.Length; j++)
                        {
                            bitArray[cont] = bits[j];

                            if(cont == 7)
                            {
                                bitArray.CopyTo(curByte, 0);
                                body.Add(curByte[0]);
                                cont = 0;
                            } 
                            else
                            {
                                cont++;
                            }
                        }
                    }

                    if(cont > 0)
                    {
                        bitArray.CopyTo(curByte, 0);
                        body.Add(curByte[0]);
                    }

                    // Insere um cabecalho no inicio com as sobras de bits usados no final, e os respectivos indices pra cada caracter
                    var header = new List<byte>() { Convert.ToByte((7 - cont).ToString()) , Convert.ToByte(';') };
                    foreach (var it in dict)
                    {
                        header.Add(Convert.ToByte(it.Key));
                        for (int k = 0; k < it.Value.Length; k++)
                        {
                            header.Add(Convert.ToByte(it.Value[k] ? '1' : '0'));
                        }

                        header.Add(Convert.ToByte(';'));
                    }
                    header.Add(Convert.ToByte(';'));
                    header.Add(Convert.ToByte(';'));


                    newFile.Write(header.Concat(body).ToArray());
                    newFile.Close();
                }
                // Descomprimindo arquivo
                else
                {
                    var readAsStr = string.Join("", file.Select(x => (char) x).ToArray());
                    var headerLen = readAsStr.IndexOf(";;;") + 1;

                    //Define o resto e a tabela de valores dos caracteres a partir do cabecalho
                    var header = readAsStr.Substring(0, headerLen).Split(";");
                    var rest = Convert.ToInt32((byte)header[0][0]);
                    var dict = new Dictionary<BitArray, byte>();

                    for (int i = 1; i < header.Length - 1; i++)
                    {
                        var curChar = (byte) header[i][0];
                        var bits = header[i].Substring(1);
                        var bitArray = new BitArray(bits.Length);
                        
                        for(int j = 0; j < bitArray.Length; j++)
                        {
                            bitArray[j] = bits[j] == '1';
                        }

                        dict.Add(bitArray, curChar);
                    }

                    var body = file.Skip(headerLen).ToArray();
                    var generalBitsArray = new BitArray(body);

                    var convertedBytes = new List<byte>();

                    var seq = new List<bool>();

                    for(int k = 0; k < generalBitsArray.Length - rest; k++)
                    {
                        seq.Add(generalBitsArray[k]);
                        var valueFound = dict.Where(x => {
                            var array = x.Key;

                            if(array.Length != seq.Count())
                            {
                                return false;
                            }
                            
                            for(int i = 0 ; i < array.Length; i++)
                            {
                                if (array[i] != seq[i])
                                {
                                    return false;
                                }
                            }

                            return true;
                        });

                        if (valueFound != null && valueFound.Count() > 0)
                        {
                            convertedBytes.Add(valueFound.First().Value);
                            seq.Clear();
                        }
                    }

                    // Cria novo arquivo para armazenar os dados compactados
                    var filePathSplitted = filePath.Split(@"\");
                    filePathSplitted[filePathSplitted.GetUpperBound(0)] = filePathSplitted[filePathSplitted.GetUpperBound(0)].Replace(".txt", "-DEC.txt");
                    var newFile = File.Create(string.Join(@"\", filePathSplitted));

                    newFile.Write(convertedBytes.ToArray());
                    newFile.Close();
                }
            }
            else
            {
                // Erro ao tentar abrir arquivo informado
                throw new Exception("Nao foi possivel abrir o arquivo, verifique se o caminho esta correto e tente novamente.");
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.StackTrace);
            Console.Error.WriteLine(e.Message);
        }
        finally
        {
            Console.WriteLine("Fim do programa.");
        }

    }

    //Ordena lista com base no valor da frequencia do no em ordem crescente
    static void SortList(ref List<BinaryTreeNode> list)
    {
        int stop = list.Count() - 1;
        while (stop >= 2)
        {
            for (int i = 0; i < stop; i++)
            {
                if (list[i].Freq > list[i + 1].Freq)
                {
                    BinaryTreeNode aux = list[i];
                    list[i] = list[i + 1];
                    list[i + 1] = aux;
                }
            }

            stop--;
        }
    }

    //Adiciona os ultimos nos da arvore a um dicionario definindo o novo valor do caracter comprimido
    static void getLeafs(ref Dictionary<byte, BitArray> dict, BinaryTreeNode node, BitArray? array = null)
    {
        if (node == null)
        {
            return;
        }

        if(node.Left == null && node.Right == null)
        {
            if(node.Val != null && array != null)
            {
                dict.Add(node.Val.Value, array);
            }
        } 
        else
        {
            var bitArray = new BitArray(1);

            if (array != null)
            {
                bitArray = new BitArray(array.Length + 1);
                for (int i = 0; i < array.Length; i++)
                {
                    bitArray[i] = array[i];
                }
            }

            if(node.Left != null)
            {
                getLeafs(ref dict, node.Left, new BitArray(bitArray));
            }

            if (node.Right != null)
            {
                bitArray[bitArray.Length - 1] = true;
                getLeafs(ref dict, node.Right, new BitArray(bitArray));
            }
        }
    }
}