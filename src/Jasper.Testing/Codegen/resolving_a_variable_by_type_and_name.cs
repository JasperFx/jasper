using System;
using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using Xunit;

namespace Jasper.Testing.Codegen
{
    public class resolving_a_variable_by_type_and_name
    {
        [Fact]
        public void matches_one_of_the_arguments()
        {
            var arg1 = new Argument(typeof(string), "foo");
            var arg2 = new Argument(typeof(string), "bar");

            var frame1 = new FrameThatBuildsVariable("aaa", typeof(string));

            var method = new GeneratedMethod("Something", new Argument[]{arg1, arg2}, new List<Frame>{frame1} );
            method.FindVariableByName(typeof(string), "foo")
                .ShouldBeTheSameAs(arg1);

            method.FindVariableByName(typeof(string), "bar")
                .ShouldBeTheSameAs(arg2);

        }


        [Fact]
        public void created_by_one_of_the_frames()
        {
            var arg1 = new Argument(typeof(string), "foo");
            var arg2 = new Argument(typeof(string), "bar");

            var frame1 = new FrameThatBuildsVariable("aaa", typeof(string));
            var frame2 = new FrameThatBuildsVariable("bbb", typeof(string));

            var method = new GeneratedMethod("Something", new Argument[]{arg1, arg2}, new List<Frame>{frame1, frame2} );
            method.FindVariableByName(typeof(string), "aaa")
                .ShouldBeTheSameAs(frame1.Variable);

            method.FindVariableByName(typeof(string), "bbb")
                .ShouldBeTheSameAs(frame2.Variable);
        }

        [Fact]
        public void sourced_from_a_variable_source()
        {
            var arg1 = new Argument(typeof(string), "foo");
            var arg2 = new Argument(typeof(string), "bar");

            var frame1 = new FrameThatBuildsVariable("aaa", typeof(string));
            var frame2 = new FrameThatBuildsVariable("bbb", typeof(string));

            var method = new GeneratedMethod("Something", new Argument[]{arg1, arg2}, new List<Frame>{frame1, frame2} );
            var source1 = new StubbedSource(typeof(string), "ccc");
            var source2 = new StubbedSource(typeof(string), "ddd");

            method.Sources.Add(source1);
            method.Sources.Add(source2);

            method.FindVariableByName(typeof(string), "ccc")
                .ShouldBeTheSameAs(source1.Variable);

            method.FindVariableByName(typeof(string), "ddd")
                .ShouldBeTheSameAs(source2.Variable);
        }

        [Fact]
        public void sad_path()
        {
            var arg1 = new Argument(typeof(string), "foo");
            var arg2 = new Argument(typeof(string), "bar");

            var frame1 = new FrameThatBuildsVariable("aaa", typeof(string));
            var frame2 = new FrameThatBuildsVariable("bbb", typeof(string));

            var method = new GeneratedMethod("Something", new Argument[]{arg1, arg2}, new List<Frame>{frame1, frame2} );
            var source1 = new StubbedSource(typeof(string), "ccc");
            var source2 = new StubbedSource(typeof(string), "ddd");

            method.Sources.Add(source1);
            method.Sources.Add(source2);

            Exception<ArgumentOutOfRangeException>.ShouldBeThrownBy(() =>
            {
                method.FindVariableByName(typeof(string), "missing");
            });
        }

        [Fact]
        public void sad_path_2()
        {
            var arg1 = new Argument(typeof(string), "foo");
            var arg2 = new Argument(typeof(string), "bar");

            var frame1 = new FrameThatBuildsVariable("aaa", typeof(string));
            var frame2 = new FrameThatBuildsVariable("bbb", typeof(string));

            var method = new GeneratedMethod("Something", new Argument[]{arg1, arg2}, new List<Frame>{frame1, frame2} );
            var source1 = new StubbedSource(typeof(string), "ccc");
            var source2 = new StubbedSource(typeof(string), "ddd");

            method.Sources.Add(source1);
            method.Sources.Add(source2);

            Exception<ArgumentOutOfRangeException>.ShouldBeThrownBy(() =>
            {
                method.FindVariableByName(typeof(int), "ccc");
            });
        }


    }

    public class StubbedSource : IVariableSource
    {
        public readonly Variable Variable;

        public StubbedSource(Type dependencyType, string name)
        {
            Variable = new Variable(dependencyType, name);
        }

        public bool Matches(Type type)
        {
            return type == Variable.VariableType;
        }

        public Variable Create(Type type)
        {
            return Variable;
        }
    }



    public class FrameThatNeedsVariable : Frame
    {
        private readonly string _name;
        private readonly Type _dependency;

        public FrameThatNeedsVariable(string name, Type dependency) : base(false)
        {
            _name = name;
            _dependency = dependency;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {

        }

        protected override IEnumerable<Variable> resolveVariables(GeneratedMethod chain)
        {
            Resolved = chain.FindVariableByName(_dependency, _name);
            yield return Resolved;
        }

        public Variable Resolved { get; private set; }
    }

    public class FrameThatBuildsVariable : Frame
    {
        public readonly Variable Variable;

        public FrameThatBuildsVariable(string name, Type dependency) : base(false)
        {
            Variable = new Variable(dependency, name);
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine("FrameThatBuildsVariable");
        }

        public override IEnumerable<Variable> Creates => new[] {Variable};
    }
}
