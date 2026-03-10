using AutomaticTodoList.Models;
using StardewModdingAPI.Events;
using StardewValley;

namespace AutomaticTodoList.Components.TodoItems;


internal class AnimalsTodoItem(string animalType, bool isChecked = false)
    : BaseTodoItem(isChecked, TaskPriority.Default)
{
    // each todo tracks progress for a given animal "type" (e.g. cow, chicken,
    // dog). we scan the farm on-demand so counts always reflect current state.

    public string Type { get; } = animalType;

    public override string Text()
    {
        Farm? farm = Game1.getFarm();
        if (farm == null)
            return string.Empty;

        var all = farm.getAllFarmAnimals();
        var matching = all.Where(a => a != null &&
                                     a.type.Value.Equals(this.Type, StringComparison.OrdinalIgnoreCase));
        int total = matching.Count(a => a.health.Value > 0);
        int petted = matching.Count(a => a.health.Value > 0 &&
                                         (a.wasPet.Value || a.health.Value <= 0));
        return I18n.Items_Animals_ByType_Text(petted, total, this.Type);
    }

    public override void OnUpdateTicked(UpdateTickedEventArgs e)
    {
        if (!IsChecked)
        {
            Farm? farm = Game1.getFarm();
            if (farm == null)
                return;

            bool allDone = farm.getAllFarmAnimals()
                .Where(a => a != null &&
                            a.type.Value.Equals(this.Type, StringComparison.OrdinalIgnoreCase))
                .All(a => a == null || a.health.Value <= 0 || a.wasPet.Value);
            if (allDone)
                this.MarkCompleted();
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is AnimalsTodoItem other &&
               string.Equals(this.Type, other.Type, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return (this.GetType(), this.Type.ToLowerInvariant()).GetHashCode();
    }
}
