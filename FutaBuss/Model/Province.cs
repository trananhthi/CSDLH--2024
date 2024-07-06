namespace FutaBuss.Model
{
    public class Province
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public Province(string code, string name)
        {
            Code = code;
            Name = name;
        }

    }
}
