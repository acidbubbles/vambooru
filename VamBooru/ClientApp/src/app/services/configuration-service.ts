import { Injectable, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import "rxjs/add/operator/toPromise";

import { IStartupConfiguration } from "../model/startup-configuration";

@Injectable()
export class ConfigurationService {
	private startupConfiguration: IStartupConfiguration;

	constructor(private readonly http: HttpClient, @Inject("BASE_URL") private readonly baseUrl: string) {}

	load(): Promise<IStartupConfiguration> {
		return this.http.get<IStartupConfiguration>(`${this.baseUrl}/api/startup`)
			.toPromise()
			.then((response: IStartupConfiguration) => {
				this.startupConfiguration = response;
				return this.startupConfiguration;
			})
			.catch(err => {
				return Promise.reject(err);
			});
	}

	get config(): IStartupConfiguration {
		return this.startupConfiguration;
	}
}
