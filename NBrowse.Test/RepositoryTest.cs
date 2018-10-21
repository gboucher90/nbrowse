using System;
using System.Linq;
using System.Threading.Tasks;
using NBrowse.Reflection;
using NUnit.Framework;

namespace NBrowse.Test
{
    public class RepositoryTest
    {
        [Test]
        [TestCase("project => 42", 42)]
        [TestCase("project => \"Hello, World!\"", "Hello, World!")]
        public async Task Query_Constant_ReturnLiteral<T>(string expression, T expected)
        {
            Assert.AreEqual(expected, await CreateAndQuery<T>(expression));
        }

        [Test]
        public async Task Query_SingleType_PrivateClassWithFields()
        {
            var candidateType = await FindTypeByName("PrivateClassWithFields");
            var expectedType = typeof(PrivateClassWithFields);

            Assert.AreEqual(expectedType.Name, candidateType.Name);
            Assert.AreEqual(Model.Class, candidateType.Model);
            Assert.AreEqual(expectedType.Assembly.GetName().Name, candidateType.Parent.Name);
            Assert.AreEqual(expectedType.Namespace, candidateType.Namespace);
            Assert.AreEqual(Visibility.Private, candidateType.Visibility);

            var candidateFields = candidateType.Fields.ToArray();

            Assert.AreEqual(4, candidateFields.Length);

            Assert.AreEqual(Binding.Dynamic, candidateFields[0].Binding);
            Assert.AreEqual("A", candidateFields[0].Name);
            Assert.AreEqual("String", candidateFields[0].Type.Name);
            Assert.AreEqual(Visibility.Public, candidateFields[0].Visibility);
            Assert.AreEqual(Binding.Dynamic, candidateFields[1].Binding);

            Assert.AreEqual("B", candidateFields[1].Name);
            Assert.AreEqual("Int32", candidateFields[1].Type.Name);
            Assert.AreEqual(Visibility.Protected, candidateFields[1].Visibility);
            Assert.AreEqual(Binding.Static, candidateFields[2].Binding);

            Assert.AreEqual("C", candidateFields[2].Name);
            Assert.AreEqual("Single", candidateFields[2].Type.Name);
            Assert.AreEqual(Visibility.Private, candidateFields[2].Visibility);
            Assert.AreEqual(Binding.Dynamic, candidateFields[3].Binding);

            Assert.AreEqual("D", candidateFields[3].Name);
            Assert.AreEqual("Int64", candidateFields[3].Type.Name);
            Assert.AreEqual(Visibility.Internal, candidateFields[3].Visibility);
        }

        [Test]
        public async Task Query_SingleType_PublicClassWithMethods()
        {
            var candidateType = await FindTypeByName("PublicClassWithMethods");
            var expectedType = typeof(PublicClassWithMethods);

            Assert.AreEqual(expectedType.Name, candidateType.Name);
            Assert.AreEqual(Model.Class, candidateType.Model);
            Assert.AreEqual(expectedType.Assembly.GetName().Name, candidateType.Parent.Name);
            Assert.AreEqual(expectedType.Namespace, candidateType.Namespace);
            Assert.AreEqual(Visibility.Public, candidateType.Visibility);

            var candidateMethods = candidateType.Methods.ToArray();

            Assert.AreEqual(6, candidateMethods.Length);

            Assert.AreEqual(Binding.Constructor, candidateMethods[0].Binding);
            Assert.AreEqual(Inheritance.Actual, candidateMethods[0].Inheritance);
            Assert.AreEqual(".ctor", candidateMethods[0].Name);
            Assert.AreEqual("Void", candidateMethods[0].ReturnType.Name);
            Assert.AreEqual(Visibility.Public, candidateMethods[0].Visibility);

            var candidateMethodArguments = candidateMethods[0].Arguments.ToArray();

            Assert.AreEqual(1, candidateMethodArguments.Length);
            Assert.AreEqual("index", candidateMethodArguments[0].Name);
            Assert.AreEqual("Int32", candidateMethodArguments[0].Type.Name);

            Assert.AreEqual(Binding.Dynamic, candidateMethods[1].Binding);
            Assert.AreEqual(Inheritance.Final, candidateMethods[1].Inheritance);
            Assert.AreEqual("GetHashCode", candidateMethods[1].Name);
            Assert.AreEqual("Int32", candidateMethods[1].ReturnType.Name);
            Assert.AreEqual(Visibility.Public, candidateMethods[1].Visibility);

            Assert.AreEqual(Binding.Dynamic, candidateMethods[2].Binding);
            Assert.AreEqual(Inheritance.Virtual, candidateMethods[2].Inheritance);
            Assert.AreEqual("ToString", candidateMethods[2].Name);
            Assert.AreEqual("String", candidateMethods[2].ReturnType.Name);
            Assert.AreEqual(Visibility.Public, candidateMethods[2].Visibility);

            Assert.AreEqual(Binding.Dynamic, candidateMethods[3].Binding);
            Assert.AreEqual(Inheritance.Virtual, candidateMethods[3].Inheritance);
            Assert.AreEqual("ProtectedVirtualDynamicMethod", candidateMethods[3].Name);
            Assert.AreEqual("TimeSpan", candidateMethods[3].ReturnType.Name);
            Assert.AreEqual(Visibility.Protected, candidateMethods[3].Visibility);

            Assert.AreEqual(Binding.Static, candidateMethods[4].Binding);
            Assert.AreEqual(Inheritance.Actual, candidateMethods[4].Inheritance);
            Assert.AreEqual("PrivateStaticMethod", candidateMethods[4].Name);
            Assert.AreEqual("Uri", candidateMethods[4].ReturnType.Name);
            Assert.AreEqual(Visibility.Private, candidateMethods[4].Visibility);

            Assert.AreEqual(Binding.Dynamic, candidateMethods[5].Binding);
            Assert.AreEqual(Inheritance.Abstract, candidateMethods[5].Inheritance);
            Assert.AreEqual("InternalAbstractMethod", candidateMethods[5].Name);
            Assert.AreEqual("Guid", candidateMethods[5].ReturnType.Name);
            Assert.AreEqual(Visibility.Internal, candidateMethods[5].Visibility);
        }

        [Test]
        public async Task Query_SingleType_InternalStructure()
        {
            var candidateType = await FindTypeByName("InternalStructure");
            var expectedType = typeof(InternalStructure);

            Assert.AreEqual(expectedType.Name, candidateType.Name);
            Assert.AreEqual(Model.Structure, candidateType.Model);
            Assert.AreEqual(expectedType.Assembly.GetName().Name, candidateType.Parent.Name);
            Assert.AreEqual(expectedType.Namespace, candidateType.Namespace);
            Assert.AreEqual(Visibility.Internal, candidateType.Visibility);
        }

        private static async Task<T> CreateAndQuery<T>(string expression)
        {
            var repository = new Repository(new [] { typeof(RepositoryTest).Assembly.Location });
            var untyped = await repository.Query(expression);

            if (untyped is T typed)
                return typed;

            throw new InvalidOperationException("invalid return type");
        }

        private static Task<Reflection.Type> FindTypeByName(string name)
        {
            return CreateAndQuery<Reflection.Type>($"project => project.Assemblies.SelectMany(a => a.Types).Where(t => t.Name == \"{name}\").First()");
        }

        private class PrivateClassWithFields
        {
            public string A;
            protected int B;
            private static float C;
            internal long D;
        }

        public abstract class PublicClassWithMethods
        {
            public PublicClassWithMethods(int index)
            {
            }

            public sealed override int GetHashCode()
            {
                return default(int);
            }

            public override string ToString()
            {
                return default(string);
            }

            protected virtual TimeSpan ProtectedVirtualDynamicMethod()
            {
                return default(TimeSpan);
            }

            private static Uri PrivateStaticMethod()
            {
                return default(Uri);
            }

            internal abstract Guid InternalAbstractMethod();
        }

        internal struct InternalStructure
        {}
    }
}