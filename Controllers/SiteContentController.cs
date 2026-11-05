using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Alumni_Management_System.Models.ViewModels;
using Alumni_Management_System.Services;

namespace Alumni_Management_System.Controllers
{
    // Lets Admins edit page text (Privacy, Help, footer) and email templates
    // without a code change. Stored as files - see SiteContentStore.
    [Authorize(Roles = "Admin")]
    public class SiteContentController : Controller
    {
        private readonly SiteContentStore _store;
        private readonly SiteContentRenderer _renderer;

        public SiteContentController(SiteContentStore store, SiteContentRenderer renderer)
        {
            _store = store;
            _renderer = renderer;
        }

        public IActionResult Index()
        {
            var items = SiteContentDefinitions.All
                .Select(d => (Definition: d, Saved: _store.Get(d.Key)))
                .ToList();
            return View(items);
        }

        // GET: SiteContent/Edit/privacy
        public IActionResult Edit(string id, string returnUrl)
        {
            var definition = SiteContentDefinitions.Find(id);
            if (definition == null)
            {
                return NotFound();
            }

            var saved = _store.Get(definition.Key);
            var model = new SiteContentEditViewModel
            {
                Key = definition.Key,
                Subject = saved.Subject,
                Body = saved.Body,
                ReturnUrl = returnUrl
            };
            return View(Prepare(model, definition));
        }

        // POST: SiteContent/Edit/privacy - "preview" re-shows the form with
        // the rendered result; "save" writes it.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SiteContentEditViewModel model, string submit)
        {
            var definition = SiteContentDefinitions.Find(model.Key);
            if (definition == null)
            {
                return NotFound();
            }

            model.Body ??= "";
            model.Subject = definition.HasSubject ? model.Subject?.Trim() : null;

            if (string.IsNullOrWhiteSpace(model.Body))
            {
                ModelState.AddModelError(nameof(model.Body), "Please fill in this field.");
            }
            if (definition.HasSubject && string.IsNullOrWhiteSpace(model.Subject))
            {
                ModelState.AddModelError(nameof(model.Subject), "Please fill in this field.");
            }
            foreach (var error in _renderer.Validate(definition, model.Subject, model.Body))
            {
                ModelState.AddModelError(string.Empty, error);
            }

            if (submit != "save" || !ModelState.IsValid)
            {
                return View(Prepare(model, definition));
            }

            await _store.SaveAsync(definition.Key, model.Subject, model.Body, User.Identity?.Name);
            TempData["SuccessMessage"] = $"\"{definition.Title}\" was saved.";
            return RedirectBack(model.ReturnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset(string id, string returnUrl)
        {
            var definition = SiteContentDefinitions.Find(id);
            if (definition == null)
            {
                return NotFound();
            }

            await _store.ResetAsync(definition.Key, User.Identity?.Name);
            TempData["SuccessMessage"] = $"\"{definition.Title}\" was reset to the default text.";
            return RedirectBack(returnUrl);
        }

        private SiteContentEditViewModel Prepare(SiteContentEditViewModel model, SiteContentDefinition definition)
        {
            model.Definition = definition;
            model.Saved = _store.Get(definition.Key);

            switch (definition.Kind)
            {
                case SiteContentKind.Email:
                    var samples = definition.Placeholders.ToDictionary(p => p.Name, p => p.Sample);
                    var email = _renderer.RenderEmail(model.Subject, model.Body, samples);
                    model.PreviewSubject = email.Subject;
                    model.PreviewHtml = email.HtmlBody;
                    break;
                case SiteContentKind.PlainText:
                    model.PreviewHtml = WebUtility.HtmlEncode(_renderer.RenderPlainText(model.Body));
                    break;
                default:
                    model.PreviewHtml = _renderer.RenderPageMarkdown(model.Body);
                    break;
            }
            return model;
        }

        private IActionResult RedirectBack(string returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction(nameof(Index));
    }
}
