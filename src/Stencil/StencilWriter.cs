using System.Collections.Generic;

namespace Stencil
{
    public class StencilWriter
    {
        List<char> characters;

        public List<char> Characters => characters;

        public char LastChar { get; private set; }

        public StencilWriter()
        {
            Reset();
        }

        public void Write(char value)
        {
            if (value == ' ')
            {
                LastChar = ' ';
                return;
            }

            if (LastChar == ' ' && characters.Count > 0)
                characters.Add(LastChar);

            LastChar = value;
            characters.Add(value);
        }

        public void Write(string value)
        {
            var array = value.ToCharArray();

            for (int i = 0; i < array.Length; i++)
                Write(array[i]);
        }

        public void Reset()
        {
            characters = new List<char>();
        }

        public override string ToString()
        {
            return new string(characters.ToArray());
        }
    }
}
