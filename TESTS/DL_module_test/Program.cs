using Vorcyc.Mathematics.DeepLearning.Layers;
using Vorcyc.Mathematics.LinearAlgebra;

namespace DL_module_test
{
    class Program
    {
        static void Main(string[] args)
        {
            TestUpsample2D_FloatTensor();
            TestUpsample2D_GenericTensor();
        }

        static void TestUpsample2D_FloatTensor()
        {
            // Arrange
            var inputArray = new float[,,]
            {
                    { { 1 }, { 2 } },
                    { { 3 }, { 4 } }
            };
            var inputTensor = new Tensor(inputArray);

            var expectedArray = new float[,,]
            {
                    { { 1 }, { 1 }, { 2 }, { 2 } },
                    { { 1 }, { 1 }, { 2 }, { 2 } },
                    { { 3 }, { 3 }, { 4 }, { 4 } },
                    { { 3 }, { 3 }, { 4 }, { 4 } }
            };
            var expectedTensor = new Tensor(expectedArray);

            // Act
            var resultTensor = Layers.Upsample2D(inputTensor);

            // Assert
            bool isCorrect = true;
            for (int d = 0; d < expectedTensor.Depth; d++)
            {
                for (int y = 0; y < expectedTensor.Height; y++)
                {
                    for (int x = 0; x < expectedTensor.Width; x++)
                    {
                        if (expectedTensor[x, y, d] != resultTensor[x, y, d])
                        {
                            isCorrect = false;
                            break;
                        }
                    }
                }
            }

            Console.WriteLine($"Upsample2D_FloatTensor test passed: {isCorrect}");
        }

        static void TestUpsample2D_GenericTensor()
        {
            // Arrange
            var inputArray = new double[,,]
            {
                    { { 1.0 }, { 2.0 } },
                    { { 3.0 }, { 4.0 } }
            };
            var inputTensor = new Tensor<double>(inputArray);

            var expectedArray = new double[,,]
            {
                    { { 1.0 }, { 1.0 }, { 2.0 }, { 2.0 } },
                    { { 1.0 }, { 1.0 }, { 2.0 }, { 2.0 } },
                    { { 3.0 }, { 3.0 }, { 4.0 }, { 4.0 } },
                    { { 3.0 }, { 3.0 }, { 4.0 }, { 4.0 } }
            };
            var expectedTensor = new Tensor<double>(expectedArray);

            // Act
            var resultTensor = Layers.Upsample2D(inputTensor);

            // Assert
            bool isCorrect = true;
            for (int d = 0; d < expectedTensor.Depth; d++)
            {
                for (int y = 0; y < expectedTensor.Height; y++)
                {
                    for (int x = 0; x < expectedTensor.Width; x++)
                    {
                        if (expectedTensor[x, y, d] != resultTensor[x, y, d])
                        {
                            isCorrect = false;
                            break;
                        }
                    }
                }
            }

            Console.WriteLine($"Upsample2D_GenericTensor test passed: {isCorrect}");
        }
    }
}
