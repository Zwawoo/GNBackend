using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Util.Helper
{
    public class StringHelper
    {
        const string TaggedUserPattern = @"(?<=@)\w+";
        public static List<string> GetTaggedUsers(string text)
        {
            var resultList = new List<string>();
            MatchCollection collection = Regex.Matches(text, TaggedUserPattern, RegexOptions.Singleline);
            foreach (Match item in collection)
            {
                resultList.Add(item.Value);
            }
            return resultList.Distinct().ToList();
        }
    }
}
