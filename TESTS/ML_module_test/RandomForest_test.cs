using Vorcyc.Mathematics.MachineLearning;

namespace ML_module_test;

internal class RandomForest_test
{


    public static void go()
    {
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

        var randomForest = new RandomForest(10, 2);
        randomForest.Train(data, attributes);

        var instance = new Dictionary<string, string>
            {
                { "Outlook", "Sunny" },
                { "Temperature", "Cool" },
                { "Humidity", "Normal" },
                { "Wind", "Weak" }
            };

        var prediction = randomForest.Predict(instance);
        Console.WriteLine($"Prediction: {prediction}");
    }
}
