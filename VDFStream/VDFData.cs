using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Indieteur.VDFAPI
{
    public class VdfData
    {
        #region Constructors

        public VdfData()
        {
            VdfParser = new VdfParser(this);
            Nodes = new List<VdfNode>();
        }

        public VdfData(VdfNode nodeToAdd)
        {
            VdfParser = new VdfParser(this);
            Nodes = new List<VdfNode>(1); //We are adding nodeToAdd to the list so let's explicitly instantiate a List<Node> with the length of 1 element.
            nodeToAdd.ParentVdfStructure = this;
            Nodes.Add(nodeToAdd);
        }

        public VdfData(IEnumerable<VdfNode> nodesToAdd)
        {
            VdfParser = new VdfParser(this);
            Nodes = new List<VdfNode>(nodesToAdd);
            foreach(VdfNode node in Nodes) //Change the ParentVDFStructure property of each of the added nodes to this instance of VDFData.
            {
                node.ParentVdfStructure = this;
            }
        }

        public VdfData(string data)
        {
            VdfParser = new VdfParser(this);
            parseData(data);
        }

        public VdfData(char[] stream)
        {
            VdfParser = new VdfParser(this);
            VdfParser.ParseData(stream);
        }

        #endregion

        #region Properties

        public List<VdfNode> Nodes { get; private set; }

        private VdfParser VdfParser { get; }

        #endregion

        #region Static Methods

        public static bool TryParseFile(string path, out VdfData result)
        {
            try
            {
                result = new VdfData();
                result.LoadDataFromFile(path);
            }
            catch
            {
                result = null;
                return false;
            }

            return true;
        }

        #endregion

        #region Methods

        public void LoadDataFromFile(string path)
        {
            if(!File.Exists(path))
            {
                throw new FileNotFoundException($"File {path} is not found!");
            }

            VdfParser.ParseData(File.ReadAllText(path).ToCharArray()); //Read all the contents of the file and convert it into a char array which we can then parse using the LoadData function.
        }

        public void SaveToFile (string filePath, bool overwrite = false)
        {
            SaveToFile(filePath, Delimiters.SystemDefault, 0, overwrite);
        }

        public void SaveToFile (string filePath, string delimiter, int startingTabLevel = 0, bool overwrite = false)
        {
            if (!overwrite && File.Exists(filePath))
            {
                throw new VdfStreamException($"File {filePath} already exists!");
            }

            File.WriteAllText(filePath, toString(delimiter, startingTabLevel));
        }

        public override string ToString()
        {
            return toString(Delimiters.SystemDefault); //Call the other ToString overload method, pass on a default value for the delimiter argument.
        }

        private void parseData(string data)
        {
            VdfParser.ParseData(data.ToCharArray()); //We just need to convert the data to a character array and then parse it using the LoadData method.
        }

        private string toString(string delimiter, int startingTabLevel = 0)
        {
            var sb = new StringBuilder();
            
            if (Nodes != null)
            {
                for (var i = 0; i < Nodes.Count; ++i)
                {
                    if (i == Nodes.Count - 1) //We are on the final node. Set the strDelimiter to "" or empty so that we don't have a useless character at the end of the string.
                    {
                        delimiter = "";
                    }
                    sb.Append(Nodes[i].ToString(delimiter, startingTabLevel) + delimiter); //We must make sure that the child nodes have the same styling as their parent node so pass on the delimiter and the starting tab level.
                }
            }

            return sb.ToString(); 
        }

        #endregion
    }
}

