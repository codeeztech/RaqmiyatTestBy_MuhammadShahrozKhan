using FileProcessing.WebAPI.Data;
using FileProcessing.WebAPI.Models;
using FileProcessing.WebAPI.SeedData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

[ApiController]
[Route("api/file")]
public class FileController : ControllerBase
{
    private readonly FileProcessingContext _context;

    public FileController(FileProcessingContext context)
    {
        _context = context;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeFile([FromBody] byte[] fileContent)
    {
        try
        {
            var xmlString = Encoding.UTF8.GetString(fileContent);
            var users = ParseUsersFromXml(xmlString);

            if (!users.Any())
            {
                return BadRequest("No users found in the provided XML.");
            }

            var statistics = new StatisticalModel
            {
                MinRate = users.Min(u => u.Rate),
                MaxRate = users.Max(u => u.Rate),
                AverageRate = (float)users.Average(u => u.Rate)
            };

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("adduser")]
    public async Task<IActionResult> AddUsersToFile([FromBody] AddUsersToFileModelForm model)
    {
        try
        {
            var xmlString = Encoding.UTF8.GetString(Convert.FromBase64String(model.Content));
            var existingUsers = ParseUsers1FromXml(xmlString);

            var newUsers = model.Users.Select(u => new User
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
               
                Rate = u.Rate // This can be updated with actual age if available
            });

            await _context.Users.AddRangeAsync(newUsers);
            await _context.SaveChangesAsync();

            var combinedUsers = existingUsers.Concat(newUsers).ToList();
            var newXmlString = GenerateXmlFromUsers(combinedUsers);
            return Ok(newXmlString);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    private List<XMLUser> ParseUsersFromXml(string xmlString)
    {
        var users = new List<XMLUser>();
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlString);
        var userCollection = xmlDoc.SelectSingleNode("UserCollection");
        var usersNodes = userCollection.SelectNodes("User");
        foreach (XmlElement xmlElement in usersNodes)
        {
            users.Add(new XMLUser
            {
                Email = xmlElement.SelectSingleNode("email").InnerText,
                FirstName = xmlElement.SelectSingleNode("first_name").InnerText,
                LastName = xmlElement.SelectSingleNode("last_name").InnerText,
                Rate = Convert.ToInt32(xmlElement.SelectSingleNode("rate").InnerText)
            });
        }

        return users;
    }

    private List<User> ParseUsers1FromXml(string xmlString)
    {
        var users = new List<User>();
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlString);
        var userCollection = xmlDoc.SelectSingleNode("UserCollection");
        var usersNodes = userCollection.SelectNodes("User");
        foreach (XmlElement xmlElement in usersNodes)
        {
            users.Add(new User
            {
                Id = int.Parse(xmlElement.SelectSingleNode("id").InnerText),
                Email = xmlElement.SelectSingleNode("email").InnerText,
                FirstName = xmlElement.SelectSingleNode("first_name").InnerText,
                LastName = xmlElement.SelectSingleNode("last_name").InnerText,
             
                Rate =Convert.ToInt32( xmlElement.SelectSingleNode("rate").InnerText), 
            });
        }

        return users;
    }

    private string GenerateXmlFromUsers(List<User> users)
    {
        var xmlDoc = new XmlDocument();
        var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(xmlDeclaration);

        var rootNode = xmlDoc.CreateElement("UserCollection");
        xmlDoc.AppendChild(rootNode);

        foreach (var user in users)
        {
            var userNode = xmlDoc.CreateElement("User");

           // var idNode = xmlDoc.CreateElement("id");
          //  idNode.InnerText = user.Id.ToString();
           // userNode.AppendChild(idNode);

            var firstNameNode = xmlDoc.CreateElement("first_name");
            firstNameNode.InnerText = user.FirstName;
            userNode.AppendChild(firstNameNode);

            var lastNameNode = xmlDoc.CreateElement("last_name");
            lastNameNode.InnerText = user.LastName;
            userNode.AppendChild(lastNameNode);

            var emailNode = xmlDoc.CreateElement("email");
            emailNode.InnerText = user.Email;
            userNode.AppendChild(emailNode);

            var rateNode = xmlDoc.CreateElement("rate");
            rateNode.InnerText = user.Rate.ToString();
            userNode.AppendChild(rateNode);

            rootNode.AppendChild(userNode);
        }

        using (var stringWriter = new StringWriter())
        {
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                xmlDoc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }
    }
}
