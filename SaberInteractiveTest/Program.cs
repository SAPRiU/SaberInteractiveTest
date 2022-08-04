namespace SaberInteractiveTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
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

        public void Serialize(Stream s)
        {

        }

        public void Deserialize(Stream s)
        {
        }
    }

}