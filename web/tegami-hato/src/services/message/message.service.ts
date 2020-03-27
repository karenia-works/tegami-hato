import { HttpClient } from "@angular/common/http";
import { Injectable } from '@angular/core';
import { environment } from "src/environments/environment";
import { apiConfig } from "src/environments/backend-config";
import { RecentMessageViewItem, HatoMessage } from "src/models/message";

@Injectable({
  providedIn: 'root'
})
export class MessageService {

  constructor(
    private httpClient: HttpClient
  ) { }

  getRecentMessages() {
    try {
      return this.httpClient
        .get(
          environment.endpoint + apiConfig.endpoints.message.recent
        );
    } catch (e) {
      throw new Error(
        "Failed to get recent messages. Reason: " + JSON.stringify(e.error)
      );
    }
  }
}
