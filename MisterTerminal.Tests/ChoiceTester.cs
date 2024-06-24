namespace MisterTerminal.Tests;

[TestClass]
public class ChoiceTester
{
    [TestClass]
    public class DefaultConstructor : Tester
    {
        [TestMethod]
        public void Always_SetTextToEmpty()
        {
            //Arrange

            //Act
            var instance = new Choice();

            //Assert
            instance.Text.Should().BeEmpty();
        }

        [TestMethod]
        public void Always_SetEmptyAction()
        {
            //Arrange

            //Act
            var instance = new Choice();

            //Assert
            instance.Action.Should().NotBeNull();
        }

        [TestMethod]
        public void Always_EmptyActionShouldNotDoAnything()
        {
            //Arrange
            var instance = new Choice();

            //Act
            var action = () => instance.Action.Invoke();

            //Assert
            action.Should().NotThrow();
        }
    }

    [TestClass]
    public class Constructor_Text_Action : Tester
    {
        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(null)]
        public void WhenTextIsEmpty_Throw(string text)
        {
            //Arrange

            //Act
            var action = () => new Choice(text, Dummy.Create<Action>());

            //Assert
            action.Should().Throw<ArgumentNullException>().WithParameterName("text");
        }

        [TestMethod]
        public void WhenActionIsNull_Throw()
        {
            //Arrange

            //Act
            var action = () => new Choice(Dummy.Create<string>(), null!);

            //Assert
            action.Should().Throw<ArgumentNullException>().WithParameterName("action");
        }

        [TestMethod]
        public void WhenTextIsNotEmpty_SetText()
        {
            //Arrange
            var text = Dummy.Create<string>();
            var action = Dummy.Create<Action>();

            //Act
            var result = new Choice(text, action);

            //Assert
            result.Text.Should().Be(text);
        }

        [TestMethod]
        public void WhenActionIsNotNull_SetAction()
        {
            //Arrange
            var text = Dummy.Create<string>();
            var action = Dummy.Create<Action>();

            //Act
            var result = new Choice(text, action);

            //Assert
            result.Action.Should().BeSameAs(action);
        }
    }
}