namespace Wellbeing.API.Providers
{
    public class RecommendationProvider
    {
        public string Recommendation { get; set; }
        public RecommendationProvider(string name, int score)
        {
            Recommendation = score switch
            {
                1 => $"Oh no {name}! You seem to be very upset! How about having a little chat with one of our RealMates?",
                2 => $"Oh {name}! You don't sound very well! How about taking a little break from work?",
                3 => $"Hello {name}! You seem to be doing fine! Another cuppa will do no harm! ",
                4 => $"Hi {name}! You are feeling great! You certainly are spreading the good vibes!",
                _ => $"Hey {name}! You are feeling on top of the world!! Nothing can stop you now!",
            };
        }
    }

}

