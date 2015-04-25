using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSourceScrumTool.Models.ViewModels;

namespace ScrumToolTests
{
    [TestClass]
    public class UnitTestProjectViewModels
    {
        [TestMethod]
        public void TestCreateProjectValidation_Blank()
        {
            var createViewModel = new ProjectCreateViewModel();

            //Test
            var context = new ValidationContext(createViewModel, null, null);
            var results = new List<ValidationResult>();
            var isModelStateValid = Validator.TryValidateObject(createViewModel, context, results, true);

            //Assert (Should Fail)
            Assert.IsFalse(isModelStateValid);
        }

        [TestMethod]
        public void TestCreateProjectValidation_Description()
        {
            var createViewModel = new ProjectCreateViewModel { Description = "Hello World" };

            //Test
            var context = new ValidationContext(createViewModel, null, null);
            var results = new List<ValidationResult>();
            var isModelStateValid = Validator.TryValidateObject(createViewModel, context, results, true);

            //Assert (Should Fail)
            Assert.IsFalse(isModelStateValid);
        }

        [TestMethod]
        public void TestCreateProjectValidation_Title()
        {
            //Sprint Duration has default value
            var createViewModel = new ProjectCreateViewModel { Title = "Hello World" };

            //Test
            var context = new ValidationContext(createViewModel, null, null);
            var results = new List<ValidationResult>();
            var isModelStateValid = Validator.TryValidateObject(createViewModel, context, results, true);

            //Assert (Should Pass)
            Assert.IsTrue(isModelStateValid);
        }

        [TestMethod]
        public void TestCreateProjectValidation_Duration()
        {
            var createViewModel = new ProjectCreateViewModel { SprintDuration = 1 };

            //Test
            var context = new ValidationContext(createViewModel, null, null);
            var results = new List<ValidationResult>();
            var isModelStateValid = Validator.TryValidateObject(createViewModel, context, results, true);

            //Assert (Should Fail)
            Assert.IsFalse(isModelStateValid);
        }

        [TestMethod]
        public void TestCreateProjectValidation_TitleAndDuration()
        {
            var createViewModel = new ProjectCreateViewModel
            {
                Title = "Hello World",
                SprintDuration = 1
            };

            //Test
            var context = new ValidationContext(createViewModel, null, null);
            var results = new List<ValidationResult>();
            var isModelStateValid = Validator.TryValidateObject(createViewModel, context, results, true);

            //Assert (Should Pass)
            Assert.IsTrue(isModelStateValid);
        }
    }
}
