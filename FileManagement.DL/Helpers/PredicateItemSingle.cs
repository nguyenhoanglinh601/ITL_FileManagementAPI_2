
using FileManagement.DL.Helpers.Interfaces;

namespace FileManagement.DL.Helpers.Framework
{
    /// <summary>
    /// Item for single predicate (e.g. "44")
    /// </summary>
    public class PredicateItemSingle : IPredicateItem
    {
        public PredicateItemSingle()
        {

        }

        public bool IsValid => Value != null;


        public object Value { get; set; }
    }
}