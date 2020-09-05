
namespace Weatherlog.Models.Parameters
{
    struct CloudMetarDescription
    {
        public readonly string descriptionAbbr;
        public readonly int height;
        public CloudMetarDescription(string descriptionAbbr, int height)
        {
            this.descriptionAbbr = descriptionAbbr;
            this.height = height;
        }
    }
}
