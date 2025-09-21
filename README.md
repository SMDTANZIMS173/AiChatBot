Project Overview:
A chatbot that answers student questions about admissions, courses, timings, etc., using local FAQs and OpenAI GPT for AI-generated answers.

Tech Stack:

ASP.NET Core MVC

C#

OpenAI API

JSON file for local FAQs

How it Works:

User types a question in the chat UI.

The system first checks local FAQs.

If no answer is found, it calls OpenAI GPT for a response.

Answers are displayed in the chat, keeping the conversation history.

Key Features:

Continuous conversation memory (like ChatGPT).

RAG-style AI: combines local knowledge + AI answers.

Easy to extend with more FAQs or OpenAI features.

Future Improvements:

Store conversations in a database.

Support file/document upload for context.

Improve UI styling for responsive chat experience.
