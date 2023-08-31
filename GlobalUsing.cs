global using System.Security.Cryptography;
global using System.Text;
global using System.Security.Claims;
global using System.ComponentModel.DataAnnotations;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Mvc;


// 3rd party directives
global using AutoMapper;
//using CloudinaryDotNet;
//using CloudinaryDotNet.Actions;



global using MyJob.Data;
global using MyJob.DTOs;
global using MyJob.Interfaces;
global using MyJob.Entities;
global using MyJob.Extensions;


namespace MyJob;
public static class Globals
{
    public static string GmailCode { get; set; }
}