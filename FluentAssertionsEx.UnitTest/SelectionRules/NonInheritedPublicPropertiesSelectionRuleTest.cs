using FluentAssertions;
using HivePeople.FluentAssertionsEx.SelectionRules;
using NUnit.Framework;

namespace FluentAssertionsEx.UnitTest.SelectionRules
{
    [TestFixture]
    public class NonInheritedPublicPropertiesSelectionRuleTest
    {
        class Base
        {
            public string InheritedProp { get; set; }
        }

        class Leaf : Base
        {
            public Leaf() : this(0) { }

            public Leaf(int privatePropValue)
            {
                this.PrivateProp = privatePropValue;
            }

            private int PrivateProp { get; set; }
            public int PublicField;
            public string DeclaredPublicProp { get; set; }
        }

        [Test]
        public void ExcludesInheritedProperties()
        {
            var leaf1 = new Leaf
            {
                InheritedProp = "thisIsNotEqual",
                DeclaredPublicProp = "thisIsEqual"
            };

            var leaf2 = new Leaf
            {
                InheritedProp = "toThisOtherValue",
                DeclaredPublicProp = "thisIsEqual"
            };

            leaf1.ShouldBeEquivalentTo(leaf2, options => options.Using(new NonInheritedPublicPropertiesSelectionRule()));
        }

        [Test]
        public void ExcludesPrivateProperties()
        {
            var leaf1 = new Leaf(privatePropValue: 1)
            {
                DeclaredPublicProp = "same"
            };
            var leaf2 = new Leaf(privatePropValue: 2)
            {
                DeclaredPublicProp = "same"
            };

            leaf1.ShouldBeEquivalentTo(leaf2, options => options.Using(new NonInheritedPublicPropertiesSelectionRule()));
        }

        [Test]
        public void CannotCombineWithOtherSelectionRules()
        {
            var leaf1 = new Leaf
            {
                PublicField = 1,
                InheritedProp = "thisIsEqual",
                DeclaredPublicProp = "thisIsEqual"
            };

            var leaf2 = new Leaf
            {
                PublicField = 2,
                InheritedProp = "thisIsEqual",
                DeclaredPublicProp = "thisIsEqual"
            };

            // This should fail because PublicField is different
            leaf1.ShouldBeEquivalentTo(leaf2, options => options.IncludingFields().Using(new NonInheritedPublicPropertiesSelectionRule()));

            // Order doesn't matter, it still doesn't fail as it should
            leaf1.ShouldBeEquivalentTo(leaf2, options => options.Using(new NonInheritedPublicPropertiesSelectionRule()).IncludingFields());
        }
    }
}
