using AutomaticTodoList.Components.TodoItems;
using AutomaticTodoList.Models;
using StardewValley;

namespace AutomaticTodoList.Engines;

internal class AnimalsEngine(
    Action<string, StardewModdingAPI.LogLevel> log,
    Func<bool> isEnabled
) : BaseEngine<AnimalsTodoItem>(log, isEnabled, Frequency.OnceADay)
{
    public override void UpdateItems()
    {
        // group the animals by their type string and add an item for each kind
        // that still has unpetted, living members. this gives X/Y cows, chickens,
        // etc. instead of a single "all animals" entry.
        Farm? farm = Game1.getFarm();
        if (farm is null)
            return;

        var all = farm.getAllFarmAnimals();
        // build a dictionary type -> list of animals of that type
        var byType = all
            .Where(a => a != null && a.health.Value > 0)
            .GroupBy(a => a.type.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase);

        foreach (var group in byType)
        {
            string type = group.Key;
            int total = group.Count();
            int unpetted = group.Count(a => !a.wasPet.Value);
            if (total > 0 && unpetted > 0)
                items.Add(new AnimalsTodoItem(type));
        }
    }
}
