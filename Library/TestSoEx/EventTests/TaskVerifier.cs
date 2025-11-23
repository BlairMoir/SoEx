namespace SoEx.TestSoEx.EventTests
{
    public static class TaskVerifier
    {
        public static async Task<int> CountSuccessfulTasks(params Task[] tasks)
        {
            int successes = 0;
            foreach (Task task in tasks)
            {
                try
                {
                    await task;
                    successes++;
                }
                catch (Exception)
                {
                    // swallow
                }
            }
            return successes;
        }
    }
}
