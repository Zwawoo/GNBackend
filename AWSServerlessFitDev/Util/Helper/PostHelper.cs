using AWSServerlessFitDev.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AWSServerlessFitDev.Util.Helper
{
    public class PostHelper
    {

        public static IEnumerable<Post> AddAdsToPosts(IEnumerable<Post> postList, IEnumerable<Post> adList, int interval)
        {
            if (postList == null || adList == null || interval <= 0)
                return postList;

            if(postList.Any() == false || adList.Any() == false) return postList;

            var result = new List<Post>();

            Random r = new Random();
            var sourceIndex = 0;
            do
            {
                result.Add(postList.ElementAt(sourceIndex));
                sourceIndex++;
                if (sourceIndex % interval == 0)
                {
                    result.Add(adList.ElementAt(r.Next(0, adList.Count()) ) );
                }
            } while (sourceIndex < postList.Count());

            return result;
        }
    }
}
