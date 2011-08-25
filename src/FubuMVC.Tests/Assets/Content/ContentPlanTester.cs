using System;
using FubuMVC.Core.Assets.Content;
using FubuMVC.Core.Assets.Files;
using NUnit.Framework;
using FubuTestingSupport;
using System.Linq;
using FubuCore;
using System.Collections.Generic;

namespace FubuMVC.Tests.Assets.Content
{
    [TestFixture]
    public class ContentPlanTester
    {
        [Test]
        public void initial_source_for_asset_file_with_a_path_is_a_read_file_source()
        {
            var assetFile = new AssetFile("something.js"){
                FullPath = "something"
            };

            var source = ContentPlan.InitialSourceForAssetFile(assetFile)
                .ShouldBeOfType<FileRead>();

            source.Files.Single().ShouldBeTheSameAs(assetFile);
        }

        [Test]
        public void InitialSourceForAssetFile_needs_to_throw_out_of_range_exception_if_there_is_no_full_path_for_the_asset_for_now()
        {
            var assetFile = new AssetFile("something.js");
            assetFile.FullPath.IsEmpty();


            Exception<ArgumentOutOfRangeException>.ShouldBeThrownBy(() =>
            {
                ContentPlan.InitialSourceForAssetFile(assetFile);
            });
        }

        [Test]
        public void constructor_sets_up_the_default_content_sources_for_each_file()
        {
            var files = new AssetFile[]{
                new AssetFile("something.js"){FullPath = "something.js"}, 
                new AssetFile("something2.js"){FullPath = "something2.js"}, 
                new AssetFile("something3.js"){FullPath = "something3.js"}, 
                new AssetFile("something4.js"){FullPath = "something4.js"}, 
            };

            var plan = new ContentPlan("a plan", files);

            plan.AllSources.ShouldHaveTheSameElementsAs(files.Select(x => new FileRead(x)));
        }

        [Test]
        public void find_for_file_initially()
        {
            var files = new AssetFile[]{
                new AssetFile("something.js"){FullPath = "something.js"}, 
                new AssetFile("something2.js"){FullPath = "something2.js"}, 
                new AssetFile("something3.js"){FullPath = "something3.js"}, 
                new AssetFile("something4.js"){FullPath = "something4.js"}, 
            };

            var plan = new ContentPlan("a plan", files);

            files.Each(f =>
            {
                plan.FindForFile(f).ShouldBeOfType<FileRead>().Files.Single().ShouldBeTheSameAs(f);
            });
        }


    }

    [TestFixture]
    public class when_applying_a_transform_to_an_existing_subject
    {
        private AssetFile[] files;
        private ContentPlan thePlan;
        private IContentSource theReadFileSource;
        private IContentSource theTransformerNode;

        [SetUp]
        public void SetUp()
        {
            files = new AssetFile[]{
                new AssetFile("something.js"){FullPath = "something.js"}, 
                new AssetFile("something2.js"){FullPath = "something2.js"}, 
                new AssetFile("something3.js"){FullPath = "something3.js"}, 
                new AssetFile("something4.js"){FullPath = "something4.js"}, 
            };

            thePlan = new ContentPlan("a plan", files);
            theReadFileSource = thePlan.AllSources.ElementAt(2);
            theTransformerNode = thePlan.ApplyTransform(theReadFileSource, typeof(StubTransformer));
        }

        [Test]
        public void should_remove_the_original_content_source_from_all_sources()
        {
            thePlan.AllSources.ShouldNotContain(theReadFileSource);
        }

        [Test]
        public void the_new_transform_source_should_be_at_the_same_position_as_the_original_readfilesource()
        {
            thePlan.AllSources.ElementAt(2).ShouldBeTheSameAs(theTransformerNode);            
        }

        [Test]
        public void the_transformer_node_should_use_the_specified_transformer_type()
        {
            theTransformerNode.ShouldBeOfType<Transform<StubTransformer>>();
        }

        [Test]
        public void the_transformer_node_wraps_the_original_read_file_source()
        {
            theTransformerNode.InnerSources.Single().ShouldBeTheSameAs(theReadFileSource);
        }
    }

    [TestFixture]
    public class when_combining_a_series_of_sources
    {
        private AssetFile[] files;
        private ContentPlan thePlan;
        private IContentSource theReadFileSource;
        private IContentSource theTransformerNode;
        private Core.Assets.Content.Combination theCombination;

        [SetUp]
        public void SetUp()
        {
            files = new AssetFile[]{
                new AssetFile("something.js"){FullPath = "something.js"}, 
                new AssetFile("something2.js"){FullPath = "something2.js"}, 
                new AssetFile("something3.js"){FullPath = "something3.js"}, 
                new AssetFile("something4.js"){FullPath = "something4.js"}, 
            };

            thePlan = new ContentPlan("a plan", files);
            theCombination = thePlan.Combine(new[]{thePlan.AllSources.ElementAt(1), thePlan.AllSources.ElementAt(2)});
        }

        [Test]
        public void the_combination_should_take_the_place_of_the_inner_sources_in_the_plan()
        {
            thePlan.AllSources.ShouldHaveTheSameElementsAs(new FileRead(files[0]), theCombination, new FileRead(files[3]));
        }

        [Test]
        public void the_combination_contains_the_read_file_sources()
        {
            theCombination.InnerSources.ShouldHaveTheSameElementsAs(new FileRead(files[1]), new FileRead(files[2]));
        }
    }

    [TestFixture]
    public class when_combining_a_series_of_sources_at_the_beginning_of_the_plan
    {
        private AssetFile[] files;
        private ContentPlan thePlan;
        private IContentSource theReadFileSource;
        private IContentSource theTransformerNode;
        private Core.Assets.Content.Combination theCombination;

        [SetUp]
        public void SetUp()
        {
            files = new AssetFile[]{
                new AssetFile("something.js"){FullPath = "something.js"}, 
                new AssetFile("something2.js"){FullPath = "something2.js"}, 
                new AssetFile("something3.js"){FullPath = "something3.js"}, 
                new AssetFile("something4.js"){FullPath = "something4.js"}, 
            };

            thePlan = new ContentPlan("a plan", files);
            theCombination = thePlan.Combine(new[] { thePlan.AllSources.ElementAt(0), thePlan.AllSources.ElementAt(1) });
        }

        [Test]
        public void the_combination_should_take_the_place_of_the_inner_sources_in_the_plan()
        {
            thePlan.AllSources.ShouldHaveTheSameElementsAs(theCombination, new FileRead(files[2]), new FileRead(files[3]));
        }

        [Test]
        public void the_combination_contains_the_read_file_sources()
        {
            theCombination.InnerSources.ShouldHaveTheSameElementsAs(new FileRead(files[0]), new FileRead(files[1]));
        }
    }
}