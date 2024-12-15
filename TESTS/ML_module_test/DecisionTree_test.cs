using Vorcyc.Mathematics.MachineLearning;

namespace ML_module_test;

internal class DecisionTree_test
{


    public static void BuildTree_ShouldBuildCorrectTree()
    {
        // Arrange
        var data = new List<Dictionary<string, string>>
        {
            new() { { "Outlook", "Sunny" }, { "Temperature", "Hot" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "No" } },
            new() { { "Outlook", "Sunny" }, { "Temperature", "Hot" }, { "Humidity", "High" }, { "Wind", "Strong" }, { "Label", "No" } },
            new() { { "Outlook", "Overcast" }, { "Temperature", "Hot" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new() { { "Outlook", "Rain" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new() { { "Outlook", "Rain" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new() { { "Outlook", "Rain" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Strong" }, { "Label", "No" } },
            new() { { "Outlook", "Overcast" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Strong" }, { "Label", "Yes" } },
            new() { { "Outlook", "Sunny" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "No" } },
            new() { { "Outlook", "Sunny" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new() { { "Outlook", "Rain" }, { "Temperature", "Mild" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new() { { "Outlook", "Sunny" }, { "Temperature", "Mild" }, { "Humidity", "Normal" }, { "Wind", "Strong" }, { "Label", "Yes" } },
            new() { { "Outlook", "Overcast" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Strong" }, { "Label", "Yes" } },
            new() { { "Outlook", "Overcast" }, { "Temperature", "Hot" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new() { { "Outlook", "Rain" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Strong" }, { "Label", "No" } }
        };

        var attributes = new List<string> { "Outlook", "Temperature", "Humidity", "Wind" };

        // Act
        var tree = DecisionTree.BuildTree(data, attributes);

        // Assert
        if (tree == null) throw new Exception("Tree is null");
        if (tree.Attribute != "Outlook") throw new Exception("Root attribute is not 'Outlook'");
        if (tree.Children == null) throw new Exception("Children is null");
        if (!tree.Children.ContainsKey("Sunny")) throw new Exception("Children does not contain 'Sunny'");
        if (!tree.Children.ContainsKey("Overcast")) throw new Exception("Children does not contain 'Overcast'");
        if (!tree.Children.ContainsKey("Rain")) throw new Exception("Children does not contain 'Rain'");

        Console.WriteLine("BuildTree_ShouldBuildCorrectTree passed.");
    }

    public static void Predict_ShouldReturnCorrectLabel()
    {
        // Arrange
        var data = new List<Dictionary<string, string>>
        {
            new() { { "Outlook", "Sunny" }, { "Temperature", "Hot" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "No" } },
            new() { { "Outlook", "Sunny" }, { "Temperature", "Hot" }, { "Humidity", "High" }, { "Wind", "Strong" }, { "Label", "No" } },
            new() { { "Outlook", "Overcast" }, { "Temperature", "Hot" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new() { { "Outlook", "Rain" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new() { { "Outlook", "Rain" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new() { { "Outlook", "Rain" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Strong" }, { "Label", "No" } },
            new() { { "Outlook", "Overcast" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Strong" }, { "Label", "Yes" } },
            new() { { "Outlook", "Sunny" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Weak" }, { "Label", "No" } },
            new() { { "Outlook", "Sunny" }, { "Temperature", "Cool" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new() { { "Outlook", "Rain" }, { "Temperature", "Mild" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new() { { "Outlook", "Sunny" }, { "Temperature", "Mild" }, { "Humidity", "Normal" }, { "Wind", "Strong" }, { "Label", "Yes" } },
            new() { { "Outlook", "Overcast" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Strong" }, { "Label", "Yes" } },
            new() { { "Outlook", "Overcast" }, { "Temperature", "Hot" }, { "Humidity", "Normal" }, { "Wind", "Weak" }, { "Label", "Yes" } },
            new() { { "Outlook", "Rain" }, { "Temperature", "Mild" }, { "Humidity", "High" }, { "Wind", "Strong" }, { "Label", "No" } }
        };

        var attributes = new List<string> { "Outlook", "Temperature", "Humidity", "Wind" };
        var tree = DecisionTree.BuildTree(data, attributes);

        var instance = new Dictionary<string, string>
        {
            { "Outlook", "Sunny" },
            { "Temperature", "Cool" },
            { "Humidity", "Normal" },
            { "Wind", "Weak" }
        };

        // Act
        var prediction = tree.Predict(instance);

        // Assert
        if (prediction != "Yes") throw new Exception("Prediction is not 'Yes'");

        Console.WriteLine("Predict_ShouldReturnCorrectLabel passed.");
    }


}
