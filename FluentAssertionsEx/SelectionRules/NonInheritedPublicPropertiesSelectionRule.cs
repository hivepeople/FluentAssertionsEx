using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions.Equivalency;

namespace HivePeople.FluentAssertionsEx.SelectionRules
{
    /// <summary>
    /// Rule for including all public properties that are not inherited. Basically what you might expect
    /// IncludeAllDeclaredProperties to do.
    /// <remarks>This rule does not play nice with the other rules due to IncludesMembers returning true.</remarks>
    /// </summary>
    public class NonInheritedPublicPropertiesSelectionRule : IMemberSelectionRule
    {
        /// <summary>
        /// We have to return true here or otherwise the user would have to call ExcludeProperties first, because all
        /// public properties (including inherited ones) will be included by default.
        /// </summary>
        public bool IncludesMembers
        {
            get { return true; }
        }

        public IEnumerable<SelectedMemberInfo> SelectMembers(IEnumerable<SelectedMemberInfo> selectedMembers, ISubjectInfo context, IEquivalencyAssertionOptions config)
        {
            var publicNonInheritedProps = config.GetSubjectType(context)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Select(SelectedMemberInfo.Create);

            return selectedMembers.Union(publicNonInheritedProps);
        }

        public override string ToString()
        {
            return "Include all public non-inherited properties";
        }
    }
}
