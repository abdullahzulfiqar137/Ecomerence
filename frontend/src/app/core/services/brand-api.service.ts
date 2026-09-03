import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiService } from './api.service';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { Brand } from '../models/brand.model';

@Injectable({
  providedIn: 'root'
})
export class BrandApiService {
  constructor(private readonly api: ApiService) {}

  getBrands(): Observable<Brand[]> {
    return this.api.get<Brand[]>(API_ENDPOINTS.brands.base).pipe(
      map(res => res.data || [])
    );
  }

  getFeaturedBrands(): Observable<Brand[]> {
    return this.api.get<Brand[]>(API_ENDPOINTS.brands.featured).pipe(
      map(res => res.data || [])
    );
  }

  getBrandBySlug(slug: string): Observable<Brand> {
    return this.api.get<Brand>(API_ENDPOINTS.brands.bySlug(slug)).pipe(
      map(res => res.data)
    );
  }
}
