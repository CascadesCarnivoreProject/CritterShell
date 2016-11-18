namespace CritterShell.Critters
{
    public class ColumnDefinition
    {
        public bool IsRequired { get; private set; }
        public string Name { get; private set; }

        public ColumnDefinition(string name)
            : this(name, false)
        {
        }

        public ColumnDefinition(string name, bool isRequired)
        {
            this.IsRequired = isRequired;
            this.Name = name;
        }
    }
}
