using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OperationCrawlerApi.Pages;

public class UploadInfoSiteModel : PageModel
{
    private readonly HttpClient httpClient = new();
    private const string UserNameHeader = "agn";

    public string Message { get; set; }

    public bool IsSuccess { get; set; }

    public void OnGet()
    {        
    }

    public async Task<IActionResult> OnPostAsync(IFormFile uploadFile, string apiUrl)
    {
        if (uploadFile == null || uploadFile.Length == 0)
        {
            Message = "Please choose a file to upload.";
            IsSuccess = false;
            return Page();
        }

        if (string.IsNullOrEmpty(apiUrl))
        {
            Message = "Please select an environment.";
            IsSuccess = false;
            return Page();
        }

        httpClient.DefaultRequestHeaders.Add("userName", UserNameHeader);

        try
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(uploadFile.OpenReadStream());
            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = $"{uploadFile.FileName}",
                FileName = uploadFile.FileName
            };

            content.Add(streamContent, uploadFile.FileName, uploadFile.FileName);
            content.Add(new StringContent("true"), "is_upload_excel");
            content.Add(new StringContent("agn"), "service_name");

            var response = await httpClient.PostAsync(apiUrl, content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Message = "Upload successful!<br/>"
                    + responseBody;
                IsSuccess = true;
            }
            else
            {
                Message = $"Upload failed with status code: {(int)response.StatusCode} {response.ReasonPhrase} <br/>"
                    +responseBody;
                IsSuccess = false;
            }
        }
        catch (Exception ex)
        {
            Message = $"Error when uploading file: {ex.Message}";
            IsSuccess = false;
        }

        return Page();
    }
}