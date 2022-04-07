using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Util.Helper
{
    public class StringHelper
    {
        //const string TaggedUserPattern = @"(?<=@)\w+";
        const string TaggedUserPattern = @"(?<!\w)@[\w\.]+(?<!\.)";
        public static List<string> GetTaggedUsers(string text)
        {
            var resultList = new List<string>();
            if(text== null)
            {
                return resultList;
            }
            MatchCollection collection = Regex.Matches(text, TaggedUserPattern, RegexOptions.Singleline);
            foreach (Match item in collection)
            {
                //Remove first "@"
                resultList.Add(item.Value.Substring(1));
            }
            return resultList.Distinct().ToList();
        }
    }
}
