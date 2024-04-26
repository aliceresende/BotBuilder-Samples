const axios = require('axios');
const { ActivityHandler, MessageFactory } = require('botbuilder');
const { BlobServiceClient } = require('@azure/storage-blob');
const { Readable } = require('stream');  // This line must be at the top of your file


class EchoBot extends ActivityHandler {
    constructor() {
        super();
        this.blobServiceClient = BlobServiceClient.fromConnectionString(process.env.AZURE_STORAGE_CONNECTION_STRING);

        this.onMessage(async (context, next) => {
            // Log the incoming attachments to see what is being received
            console.log('Received attachments:', context.activity.attachments);

            if (context.activity.attachments && context.activity.attachments.length > 0) {
                // Handle all attachments
                for (const attachment of context.activity.attachments) {
                    const fileDownloadUrl = attachment.contentUrl;
                    if (fileDownloadUrl) {
                        // Download and upload the file based on its URL
                        await this.downloadAndUploadFile(fileDownloadUrl, attachment.name || "uploadithelper", context);
                    } else {
                        await context.sendActivity('The attachment does not have a valid download URL.');
                    }
                }
            } else {
                await context.sendActivity('No attachment found. Please send any file type.');
            }
            await next();
        });
    }
    async downloadAndUploadFile(downloadUrl, fileName, context) {
        try {
            const response = await axios.get(downloadUrl, { responseType: 'arraybuffer' });
            const fileBuffer = Buffer.from(response.data, 'binary');
            const fileStream = bufferToStream(fileBuffer);

            const containerClient = this.blobServiceClient.getContainerClient('uploadithelper');
            const blockBlobClient = containerClient.getBlockBlobClient(fileName);

            await blockBlobClient.uploadStream(fileStream);
            await context.sendActivity('File received and uploaded to Blob Storage successfully.');
        } catch (error) {
            console.error('Error downloading or uploading file:', error);
            await context.sendActivity('Error processing your file.');
        }
    }
}

function bufferToStream(buffer) {
    const stream = new Readable();
    stream.push(buffer);
    stream.push(null); // Indicates the end of the stream
    return stream;
}

module.exports.EchoBot = EchoBot;
