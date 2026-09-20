using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CybersecurityTrainingApplication.Web.Data;
using CybersecurityTrainingApplication.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingApplication.Web.Pages;

public class IndexModel : PageModel
{
    private readonly TrainingDbContext _db;
    public TrainingModule? Module { get; private set; }

    public IndexModel(TrainingDbContext db)
    {
        _db = db;
    }

    public async Task OnGetAsync()
    {
        Module = await _db.TrainingModules.Include(x => x.QuestionBanks).FirstOrDefaultAsync(x => x.Id == 1);
    }
}
