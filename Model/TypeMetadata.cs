namespace Model
{
    public class TypeMetadata
    {
        public string Name { get; set; }
        public List<MethodMetadata> Methods { get; set; }
        public List<PropertyMetadata> Properties { get; set; }
    }

}
