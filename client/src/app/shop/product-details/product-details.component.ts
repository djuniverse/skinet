import { Component, OnInit } from '@angular/core';
import {IProduct} from "../../shared/models/product";
import {ShopService} from "../shop.service";
import {ActivatedRoute} from "@angular/router";

@Component({
  selector: 'app-product-details',
  templateUrl: './product-details.component.html',
  styleUrls: ['./product-details.component.scss']
})
export class ProductDetailsComponent implements OnInit {
  product: IProduct;


  constructor(private shopService: ShopService, private activateRoute: ActivatedRoute) { }

  ngOnInit(): void {
    this.loadProduct()
  }

  loadProduct() {
    //+zamienia string na number
    this.shopService.getProduct(+this.activateRoute.snapshot.paramMap.get('id')).subscribe({
      next: (product) => this.product = product,
      error: (error) => console.log(error)
    })
  }

}
