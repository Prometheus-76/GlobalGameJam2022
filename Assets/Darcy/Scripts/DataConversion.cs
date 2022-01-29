using System;

public class DataConversion
{
    //Initialize conversion keys
    private string decryptionKey = "v$Q;2w5nLG#Dr?/+phfK6y=sF,YCom-}!gk%cT1dO'BH I[)A.9utx^z@N&l{:*bVajJq0eSRW3MX4UiP7(Z8E]";
    private string encryptionKey = "n-[3a^O;Km#7LpcMA2%C8tHG+ZDkP0Sgv6!yJqB]IdEuoUYNW?z}X&f$1T9s5xQiV Fe./R4){=:@'h(,b*jlrw";

    //Converts the plaintext message to an encrypted string
    public string Encrypt(string message, int strength)
    {
        //return message;

        //Convert string input to char array
        char[] charArrayMessage = message.ToCharArray();

        //Sets the starting offset and adds layers of basic complexity
        int baseOffset = strength + (charArrayMessage.Length);

        //Reverse the array
        Array.Reverse(charArrayMessage);

        //For each encoding level
        for (int conversionLevel = 0; conversionLevel < strength + baseOffset; conversionLevel++)
        {
            int offset = baseOffset;

            //For each letter in the message
            for (int letterIndex = 0; letterIndex < charArrayMessage.Length; letterIndex++)
            {
                int conversionIndex = 0;

                //Loop through every available character in the key
                for (int keyIndex = 0; keyIndex < decryptionKey.Length; keyIndex++)
                {
                    //Sets the conversion index if there is a match
                    if (charArrayMessage[letterIndex] == decryptionKey[keyIndex])
                    {
                        conversionIndex = keyIndex;
                    }
                }

                //Chooses which letter to use as the replacement based on the message length
                conversionIndex = (conversionIndex + offset) % (decryptionKey.Length);

                //Replaces the current character in the original with its counterpart from the encryption key
                charArrayMessage[letterIndex] = encryptionKey[conversionIndex];

                //Prevent duplicate patterning by incrementing offset for each character
                offset += 1;
            }
        }

        //Replace spaces and underscores for readability
        string convertedMessage = new string(charArrayMessage);
        convertedMessage = convertedMessage.Replace(" ", "_");

        //Convert char array to string and return value
        return convertedMessage;
    }

    //Converts the encrypted string to a plaintext message
    public string Decrypt(string message, int strength)
    {
        //return message;

        //Replace spaces and underscores for readability
        string strMessage = message.Replace("_", " ");
        char[] charArrayMessage = strMessage.ToCharArray();

        //Sets the starting offset and adds layers of basic complexity
        int baseOffset = -(strength + (charArrayMessage.Length));

        //For each decoding level
        for (int conversionLevel = 0; conversionLevel < strength - baseOffset; conversionLevel++)
        {
            int offset = baseOffset;

            //For each letter in the message
            for (int letterIndex = 0; letterIndex < charArrayMessage.Length; letterIndex++)
            {
                int filteredIndex = 0;
                int conversionIndex = 0;

                //Loop through evey character in the key
                for (int keyIndex = 0; keyIndex < decryptionKey.Length; keyIndex++)
                {
                    //Sets the conversion index if there is a match
                    if (charArrayMessage[letterIndex] == encryptionKey[keyIndex])
                    {
                        conversionIndex = keyIndex;
                    }
                }

                //Accounts for progressive offset, checks if it falls outside the array bounds
                if (conversionIndex + offset < 0)
                {
                    //Takes remainder in case of negative overflow
                    if (-offset >= decryptionKey.Length)
                    {
                        offset %= decryptionKey.Length;
                    }

                    //If the index lands exactly on the first element in the key array
                    if (encryptionKey.Length + ((conversionIndex + offset) % (encryptionKey.Length)) == encryptionKey.Length)
                    {
                        charArrayMessage[letterIndex] = encryptionKey[0];
                    }
                    else
                    {
                        //Checks that there is no overflow
                        charArrayMessage[letterIndex] = encryptionKey[(encryptionKey.Length + ((conversionIndex + offset) % encryptionKey.Length)) % encryptionKey.Length];
                    }
                }
                else
                {
                    //If there is no overflow then the replacement is performed
                    charArrayMessage[letterIndex] = encryptionKey[conversionIndex + offset];
                }

                //Finds the letter matching in the decoding key
                for (int letterID = 0; letterID < decryptionKey.Length; letterID++)
                {
                    //Sets the filtered index once there is a match
                    if (charArrayMessage[letterIndex] == encryptionKey[letterID])
                    {
                        filteredIndex = letterID;
                    }
                }

                //Reverse character duplication offset process
                offset -= 1;

                //Replace character with the original from the decryption key
                charArrayMessage[letterIndex] = decryptionKey[filteredIndex];
            }
        }

        //Reverse the array
        Array.Reverse(charArrayMessage);

        //Convert char array to string and return value
        string convertedMessage = new string(charArrayMessage);
        return convertedMessage;
    }
}
