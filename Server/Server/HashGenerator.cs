namespace NEA
{
    public class HashGenerator
    {
        
        /// <summary>
        /// Get's the hash code of a string.
        /// </summary>
        /// <param name="input"> The string that you want to get the hash code of</param>
        /// <returns>The strings hash code.</returns>
        public static double getStringsHash(string input)
        {
            double hash = 0;
            // first we'll take the length of the string
            hash = input.Length;
            // then we'll add the ascii code of every character to it
            for (int i = 0; i < input.Length; i++)
            {
                hash += (int)input[i];
            }

            return hash;
        }
    }
}
