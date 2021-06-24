using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace CatelogueManagement.DL.Annotations
{
    public class AnnotationResult
    {
        public Dictionary<String, List<String>> result = new Dictionary<String, List<String>>();
        public void Add(string field,string message)
        {
            if (result.ContainsKey(field))
            {
                List<String> value;
                if (result.TryGetValue(field, out value)) {
                    result.Remove(field);
                    value.Add(message);
                    result.Add(field, value);
                };
            }
            else
            {
                result.Add(field,new List<String>( message.Split("\r\n")));
            }
        }
        public Boolean Exists()
        {
            if (result.Count > 0)
            {
                return true;
            }
            return false;
        }

    }
}
