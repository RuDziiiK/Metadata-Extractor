namespace Services
{
    public class ListItem
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public override string ToString() => $"{Name} (ID: {Id})";
    }

}
