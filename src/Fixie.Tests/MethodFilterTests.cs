﻿using System.Linq;
using System.Reflection;
using Should;
using Xunit;

namespace Fixie.Tests
{
    public class MethodFilterTests
    {
        [Fact]
        public void ShouldExcludeAllMethodsByDefaultDueToAmbiguousBindingFlags()
        {
            new MethodFilter()
                .Filter(typeof(Sample))
                .ShouldBeEmpty();
        }

        [Fact]
        public void CanFilterToMethodsSatisfyingBindingFlagsVisibility()
        {
            var methods =
                new MethodFilter()
                    .Visibility(BindingFlags.Public | BindingFlags.Instance)
                    .Filter(typeof(Sample));

            methods
                .OrderBy(method => method.Name)
                .Select(method => method.Name)
                .ShouldEqual(
                    "PublicInstanceNoArgsVoid",
                    "PublicInstanceNoArgsWithReturn",
                    "PublicInstanceWithArgsVoid",
                    "PublicInstanceWithArgsWithReturn");
        }

        [Fact]
        public void ShouldFilterByAllSpecifiedConditions()
        {
            var methods =
                new MethodFilter()
                    .Visibility(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(method => method.Name.Contains("Instance"))
                    .Where(method => method.Name.Contains("No"))
                    .Filter(typeof(Sample));

            methods
                .OrderBy(method => method.Name)
                .Select(method => method.Name)
                .ShouldEqual(
                    "PublicInstanceNoArgsVoid",
                    "PublicInstanceNoArgsWithReturn");
        }

        [Fact]
        public void CanFilterToVoidMethods()
        {
            var methods =
                new MethodFilter()
                    .Visibility(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Void()
                    .Filter(typeof(Sample));

            methods
                .OrderBy(method => method.Name)
                .Select(method => method.Name)
                .ShouldEqual(
                    "PrivateInstanceNoArgsVoid",
                    "PrivateInstanceWithArgsVoid",
                    "PrivateStaticNoArgsVoid",
                    "PrivateStaticWithArgsVoid",
                    "PublicInstanceNoArgsVoid",
                    "PublicInstanceWithArgsVoid",
                    "PublicStaticNoArgsVoid",
                    "PublicStaticWithArgsVoid"
                );
        }

        [Fact]
        public void CanFilterToMethodsWithZeroParameters()
        {
            var methods =
                new MethodFilter()
                    .Visibility(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .ZeroParameters()
                    .Filter(typeof(Sample));

            methods
                .OrderBy(method => method.Name)
                .Select(method => method.Name)
                .ShouldEqual(
                    "PrivateInstanceNoArgsVoid",
                    "PrivateInstanceNoArgsWithReturn",
                    "PrivateStaticNoArgsVoid",
                    "PrivateStaticNoArgsWithReturn",
                    "PublicInstanceNoArgsVoid",
                    "PublicInstanceNoArgsWithReturn",
                    "PublicStaticNoArgsVoid",
                    "PublicStaticNoArgsWithReturn"
                );
        }

        class Sample
        {
            public static int PublicStaticWithArgsWithReturn(int x) { return 0; }
            public static int PublicStaticNoArgsWithReturn() { return 0; }
            public static void PublicStaticWithArgsVoid(int x) { }
            public static void PublicStaticNoArgsVoid() { }

            public int PublicInstanceWithArgsWithReturn(int x) { return 0; }
            public int PublicInstanceNoArgsWithReturn() { return 0; }
            public void PublicInstanceWithArgsVoid(int x) { }
            public void PublicInstanceNoArgsVoid() { }

            private static int PrivateStaticWithArgsWithReturn(int x) { return 0; }
            private static int PrivateStaticNoArgsWithReturn() { return 0; }
            private static void PrivateStaticWithArgsVoid(int x) { }
            private static void PrivateStaticNoArgsVoid() { }

            private int PrivateInstanceWithArgsWithReturn(int x) { return 0; }
            private int PrivateInstanceNoArgsWithReturn() { return 0; }
            private void PrivateInstanceWithArgsVoid(int x) { }
            private void PrivateInstanceNoArgsVoid() { }
        }
    }
}