using System.Text;

namespace SaberInteractiveTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Получение количества элементов списка от пользователя
            Console.Write("Количество элементов в списке: ");
            string sCount = Console.ReadLine();
            int count;
            if (!int.TryParse(sCount, out count) || count < 1)
            {
                Console.WriteLine("Неверный формат входных данных.");
                Console.ReadKey();
                return;
            }
            
            //Создание и заполнение списка
            ListRandom list = new ListRandom();
            for(int i = 0; i < count; i++)
            {
                list.Add(i.ToString());
            }
            list.SetRandomReferences();

            Console.WriteLine("Исходный список");
            list.Print();

            //Сериализация списка и вывод потока на экран
            MemoryStream stream = new MemoryStream();
            list.Serialize(stream);
            Console.Write(Environment.NewLine + "Сериализированный список:");
            foreach (byte b in stream.ToArray())
                Console.Write(b + " ");

            //Десериализация списка и вывод на экран
            Console.WriteLine(Environment.NewLine + Environment.NewLine + "Десериализированный список");
            ListRandom deserializedList = new ListRandom();
            deserializedList.Deserialize(stream);
            deserializedList.Print();

            Console.ReadKey();
        }
    }

    class ListNode
    {
        public ListNode Previous;
        public ListNode Next;
        public ListNode Random;
        public string Data;
    }


    class ListRandom
    {
        public ListNode Head;
        public ListNode Tail;
        public int Count;

        public void Add(string data)
        {
            ListNode newNode = new ListNode();
            newNode.Data = data;
            if(Count == 0)
            {
                Head = newNode;
                Tail = newNode;
                Count++;
            }
            else
            {
                Tail.Next = newNode;
                newNode.Previous = Tail;
                Tail = newNode;
                Count++;
            }
        }

        public void Print()
        {
            if(Count != 0)
            {
                Console.WriteLine("Вывод списка: элемент -> ссылка на случайный элемент");
                ListNode currentNode = Head;
                while(currentNode != null)
                {
                    Console.Write(currentNode.Data + " ->");
                    Console.WriteLine(currentNode.Random == null ? "null" : currentNode.Random.Data);
                    currentNode = currentNode.Next;
                }
            }
        }

        public void SetRandomReferences()
        {
            ListNode currentNode = Head;
            ListNode randomNode;
            Random r = new Random();
            int randomRef;
            while(currentNode != null)
            {
                //Получение номера случайного элемента в списке
                //-1 значение null
                randomRef = r.Next(-1, Count);
                if (randomRef == -1)
                {
                    currentNode = currentNode.Next;
                    continue;
                }

                //Получение ссылки на элемент из случайного номера
                randomNode = Head;
                for (int i = 0; i < randomRef; i++)
                    randomNode = randomNode.Next;
                currentNode.Random = randomNode;

                currentNode = currentNode.Next;
            }
        }

        public void Serialize(Stream s)
        {
            //Сериализированный список имеет следующую структуру:
            //Первые 4 байта - int(количество элементов в списке)
            //Каждый элемент разбивается на:
            //4 байта - int (ссылка на произвольный элемент в виде индекса)
            //4 байта - int (длина свойства Data в байтах)
            //N байт - string (содержимое свойства Data)

            ListNode currentNode = Head;
            byte[] count;
            byte[] randomElementRef;
            byte[] dataLength;
            byte[] data;

            count = BitConverter.GetBytes(Count);
            s.Write(count, 0, count.Length);

            while(currentNode != null)
            {
                randomElementRef = BitConverter.GetBytes(GetRandomElementID(currentNode));
                data = Encoding.UTF8.GetBytes(currentNode.Data);
                dataLength = BitConverter.GetBytes(data.Length);
                s.Write(randomElementRef, 0, randomElementRef.Length);
                s.Write(dataLength, 0, dataLength.Length);
                s.Write(data, 0, data.Length);
                currentNode = currentNode.Next;
            }
        }

        private int GetRandomElementID(ListNode node)
        {
            if (node.Random == null)
                return -1;

            //Получение номера произвольного элемента
            int id = 0;
            ListNode currentNode = node.Random;
            while(currentNode.Previous != null)
            {
                id++;
                currentNode = currentNode.Previous;
            }
            return id;
        }

        public void Deserialize(Stream s)
        {
            s.Position = 0;
            byte[] intBuffer = new byte[4];
            byte[] dataBuffer;

            //Получение количества элементов списка
            s.Read(intBuffer, 0, intBuffer.Length);
            int count = BitConverter.ToInt32(intBuffer, 0);

            //Массивы, которые используются для восстановления ссылочной структуры списка
            //Массив с элементами списка
            ListNode[] listNodes = new ListNode[count];
            //Массив с ссылками на произвольные элементы списка в виде порядковых номеров
            int[] randomReferences = new int[count];
            int dataLength;

            //Получение всех элементов списка
            for (int i = 0; i < count; i++)
            {
                s.Read(intBuffer, 0, intBuffer.Length);
                randomReferences[i] = BitConverter.ToInt32(intBuffer, 0);
                s.Read(intBuffer, 0, intBuffer.Length);
                dataLength = BitConverter.ToInt32(intBuffer, 0);
                dataBuffer = new byte[dataLength];
                s.Read(dataBuffer, 0, dataBuffer.Length);

                listNodes[i] = new ListNode();
                listNodes[i].Data = Encoding.UTF8.GetString(dataBuffer);
            }

            Count = count;
            if (count == 1)
            {
                Head = listNodes[0];
                Tail = listNodes[0];
            }
            else
            {
                SetReferences(listNodes, randomReferences);

                Head = listNodes[0];
                Tail = listNodes[count - 1];
            }
        }

        private void SetReferences(ListNode[] listNodes, int[] randomReferences)
        {
            if(listNodes.Length == 0) 
                return;

            //Восстановление ссылочной структуры списка на основе массивов
            for (int i = 0; i < listNodes.Length; i++)
            {
                if (i == 0)
                {
                    listNodes[i].Next = listNodes[i + 1];
                    if (randomReferences[i] != -1)
                        listNodes[i].Random = listNodes[randomReferences[i]];
                }
                else if (i == listNodes.Length - 1)
                {
                    listNodes[i].Previous = listNodes[i - 1];
                    if (randomReferences[i] != -1)
                        listNodes[i].Random = listNodes[randomReferences[i]];
                }
                else
                {
                    listNodes[i].Next = listNodes[i + 1];
                    listNodes[i].Previous = listNodes[i - 1];
                    if (randomReferences[i] != -1)
                        listNodes[i].Random = listNodes[randomReferences[i]];
                }
            }
        }
    }

}